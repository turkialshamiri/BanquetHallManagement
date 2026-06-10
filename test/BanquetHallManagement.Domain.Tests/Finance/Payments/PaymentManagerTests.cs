using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Xunit;

namespace BanquetHallManagement.Finance.Payments;

public class PaymentManagerTests
{
    private static readonly DateTime Now = new(2026, 6, 10, 12, 0, 0);
    private static readonly Guid HallId = Guid.NewGuid();

    [Fact]
    public async Task RecordDepositAsync_Should_Reject_Deposit_Below_30_Percent()
    {
        var reservation = CreatePendingReservation(totalPrice: 100_000m);
        var manager = CreatePaymentManager([reservation], out _);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.RecordDepositAsync(reservation.Id, 25_000m));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.PaymentDepositBelowMinimum);
        reservation.Status.ShouldBe(ReservationStatus.Pending);
    }

    [Fact]
    public async Task RecordDepositAsync_Should_Confirm_Reservation_On_Valid_Deposit()
    {
        var reservation = CreatePendingReservation(totalPrice: 100_000m);
        var manager = CreatePaymentManager([reservation], out var paymentRepository);

        var payment = await manager.RecordDepositAsync(reservation.Id, 30_000m);

        reservation.Status.ShouldBe(ReservationStatus.Confirmed);
        reservation.PaidAmount.ShouldBe(30_000m);
        payment.Amount.ShouldBe(30_000m);
        payment.PaymentType.ShouldBe(PaymentType.Deposit);
        payment.ReceiptNumber.ShouldBe("RC-2026-00001");

        await paymentRepository.Received(1).InsertAsync(
            Arg.Any<Payment>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordDepositAsync_Should_Cancel_Conflicting_Pending_Reservations()
    {
        var confirmedTarget = CreatePendingReservation(totalPrice: 100_000m);
        var conflictingPending = CreatePendingReservation(
            totalPrice: 80_000m,
            startTime: new TimeSpan(19, 0, 0),
            endTime: new TimeSpan(21, 0, 0));
        var nonConflictingPending = CreatePendingReservation(
            totalPrice: 70_000m,
            startTime: new TimeSpan(10, 0, 0),
            endTime: new TimeSpan(12, 0, 0));

        var reservations = new List<Reservation>
        {
            confirmedTarget,
            conflictingPending,
            nonConflictingPending,
        };

        var manager = CreatePaymentManager(reservations, out _);

        await manager.RecordDepositAsync(confirmedTarget.Id, 30_000m);

        confirmedTarget.Status.ShouldBe(ReservationStatus.Confirmed);
        conflictingPending.Status.ShouldBe(ReservationStatus.Cancelled);
        conflictingPending.CancellationType.ShouldBe(CancellationType.ConflictOverride);
        nonConflictingPending.Status.ShouldBe(ReservationStatus.Pending);
    }

    [Fact]
    public async Task RecordDepositAsync_Should_Reject_Payment_On_Cancelled_Reservation()
    {
        var reservation = CreatePendingReservation(totalPrice: 100_000m);
        reservation.CancelWithReason(CancellationType.Manual);

        var manager = CreatePaymentManager([reservation], out _);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.RecordDepositAsync(reservation.Id, 30_000m));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.PaymentInvalidReservationStatus);
    }

    [Fact]
    public async Task RecordDepositAsync_Should_Publish_PaymentReceivedDomainEvent()
    {
        var reservation = CreatePendingReservation(totalPrice: 100_000m);
        var manager = CreatePaymentManager([reservation], out _);

        var payment = await manager.RecordDepositAsync(reservation.Id, 30_000m);

        var domainEvent = payment.GetLocalEvents()
            .Select(record => record.EventData)
            .OfType<PaymentReceivedDomainEvent>()
            .Single();

        domainEvent.PaymentId.ShouldBe(payment.Id);
        domainEvent.ReservationId.ShouldBe(reservation.Id);
        domainEvent.Amount.ShouldBe(30_000m);
        domainEvent.PaymentType.ShouldBe(PaymentType.Deposit);
    }

    private static PaymentManager CreatePaymentManager(
        IList<Reservation> reservations,
        out IRepository<Payment, Guid> paymentRepository)
    {
        var reservationRepository = Substitute.For<IReservationRepository>();
        paymentRepository = Substitute.For<IRepository<Payment, Guid>>();

        reservationRepository.GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => reservations.First(r => r.Id == callInfo.Arg<Guid>()));

        reservationRepository.AcquireExclusiveSchedulingLockAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        reservationRepository.GetQueryableAsync()
            .Returns(Task.FromResult(reservations.AsQueryable()));

        reservationRepository.UpdateAsync(
                Arg.Any<Reservation>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Reservation>()));

        paymentRepository.InsertAsync(
                Arg.Any<Payment>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Payment>()));

        var hallRepository = Substitute.For<IRepository<Hall, Guid>>();
        hallRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new Hall
            {
                Name = "Main Hall",
                Status = HallStatus.Available,
                Capacity = 500,
                PricePerHour = 1000m,
                Location = "A",
                Description = "Test",
                Type = HallType.Wedding,
            });

        var receiptNumberGenerator = Substitute.For<IReceiptNumberGenerator>();
        receiptNumberGenerator.GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("RC-2026-00001");

        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid(), _ => Guid.NewGuid(), _ => Guid.NewGuid());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var services = new ServiceCollection();
        services.AddSingleton<IAsyncQueryableExecuter, AsyncQueryableExecuter>();
        services.AddSingleton(clock);
        services.AddSingleton(guidGenerator);

        var schedulingManager = new ReservationSchedulingManager(reservationRepository)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        var hallAvailabilityManager = new HallAvailabilityManager
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        var depositConfirmationService = new DepositConfirmationService(
            reservationRepository,
            hallRepository,
            hallAvailabilityManager,
            schedulingManager)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        var paymentManager = new PaymentManager(
            paymentRepository,
            reservationRepository,
            receiptNumberGenerator,
            depositConfirmationService)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        return paymentManager;
    }

    private static Reservation CreatePendingReservation(
        decimal totalPrice,
        TimeSpan? startTime = null,
        TimeSpan? endTime = null)
    {
        return new Reservation(Guid.NewGuid())
        {
            HallId = HallId,
            CustomerId = Guid.NewGuid(),
            EventDate = Now.Date.AddDays(7),
            StartTime = startTime ?? new TimeSpan(18, 0, 0),
            EndTime = endTime ?? new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Pending,
        };
    }
}
