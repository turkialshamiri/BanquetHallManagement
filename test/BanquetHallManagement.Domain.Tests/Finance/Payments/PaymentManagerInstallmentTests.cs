using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Finance.HallAccessCards.Events;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
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

public class PaymentManagerInstallmentTests
{
    private static readonly DateTime Now = new(2026, 6, 11, 12, 0, 0);
    private static readonly Guid HallId = Guid.NewGuid();

    [Fact]
    public async Task RecordInstallmentAsync_Should_Reject_Installment_Below_20_Percent()
    {
        var reservation = CreateConfirmedReservation(totalPrice: 100_000m, paidAmount: 30_000m);
        var manager = CreatePaymentManager([reservation], out _, []);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.RecordInstallmentAsync(reservation.Id, 15_000m));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.PaymentInstallmentBelowMinimum);
        reservation.Status.ShouldBe(ReservationStatus.Confirmed);
    }

    [Fact]
    public async Task RecordInstallmentAsync_Should_Accept_Valid_Installment_And_Track_Remaining()
    {
        var reservation = CreateConfirmedReservation(totalPrice: 100_000m, paidAmount: 30_000m);
        var manager = CreatePaymentManager([reservation], out _, []);

        var result = await manager.RecordInstallmentAsync(reservation.Id, 20_000m);

        reservation.PaidAmount.ShouldBe(50_000m);
        result.RemainingAmount.ShouldBe(50_000m);
        result.IsFullyPaid.ShouldBeFalse();
        result.HallAccessCardId.ShouldBeNull();
        result.Payment.PaymentType.ShouldBe(PaymentType.Installment);
    }

    [Fact]
    public async Task RecordInstallmentAsync_Should_Mark_FullyPaid_And_Create_HallAccessCard()
    {
        var reservation = CreateConfirmedReservation(totalPrice: 100_000m, paidAmount: 30_000m);
        var hallAccessCardStore = new List<HallAccessCard>();
        var manager = CreatePaymentManager([reservation], out _, hallAccessCardStore);

        var result = await manager.RecordInstallmentAsync(reservation.Id, 70_000m);

        reservation.Status.ShouldBe(ReservationStatus.FullyPaid);
        result.IsFullyPaid.ShouldBeTrue();
        result.RemainingAmount.ShouldBe(0m);
        result.HallAccessCardId.ShouldNotBeNull();
        result.Payment.PaymentType.ShouldBe(PaymentType.Final);

        hallAccessCardStore.Count.ShouldBe(1);
        hallAccessCardStore.Single().ReservationId.ShouldBe(reservation.Id);

        reservation.GetLocalEvents()
            .Select(record => record.EventData)
            .OfType<ReservationFullyPaidDomainEvent>()
            .ShouldHaveSingleItem();
    }

    [Fact]
    public async Task RecordInstallmentAsync_Should_Allow_Final_Payment_Below_20_Percent_When_Paying_Remaining()
    {
        var reservation = CreateConfirmedReservation(totalPrice: 100_000m, paidAmount: 88_000m);
        var manager = CreatePaymentManager([reservation], out _, []);

        var result = await manager.RecordInstallmentAsync(reservation.Id, 12_000m);

        reservation.Status.ShouldBe(ReservationStatus.FullyPaid);
        result.RemainingAmount.ShouldBe(0m);
        result.Payment.PaymentType.ShouldBe(PaymentType.Final);
    }

    [Fact]
    public async Task RecordInstallmentAsync_Should_Reject_Payment_On_Pending_Reservation()
    {
        var reservation = CreateConfirmedReservation(totalPrice: 100_000m, paidAmount: 0m);
        reservation.Status = ReservationStatus.Pending;

        var manager = CreatePaymentManager([reservation], out _, []);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.RecordInstallmentAsync(reservation.Id, 20_000m));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.PaymentInvalidReservationStatus);
    }

    private static PaymentManager CreatePaymentManager(
        IList<Reservation> reservations,
        out IRepository<Payment, Guid> paymentRepository,
        List<HallAccessCard> hallAccessCards)
    {

        var reservationRepository = Substitute.For<IReservationRepository>();
        paymentRepository = Substitute.For<IRepository<Payment, Guid>>();

        reservationRepository.GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => reservations.First(r => r.Id == callInfo.Arg<Guid>()));

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

        var hallAccessCardRepository = Substitute.For<IHallAccessCardRepository>();
        hallAccessCardRepository.FindByReservationIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var reservationId = callInfo.Arg<Guid>();
                return Task.FromResult(hallAccessCards.FirstOrDefault(card => card.ReservationId == reservationId));
            });

        hallAccessCardRepository.InsertAsync(
                Arg.Any<HallAccessCard>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var card = callInfo.Arg<HallAccessCard>();
                hallAccessCards.Add(card);
                return Task.FromResult(card);
            });

        var cardNumberGenerator = Substitute.For<ICardNumberGenerator>();
        cardNumberGenerator.GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("HAC-2026-00001");

        var receiptNumberGenerator = Substitute.For<IReceiptNumberGenerator>();
        receiptNumberGenerator.GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("RC-2026-00002");

        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid(), _ => Guid.NewGuid(), _ => Guid.NewGuid());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var services = new ServiceCollection();
        services.AddSingleton<IAsyncQueryableExecuter, AsyncQueryableExecuter>();
        services.AddSingleton(clock);
        services.AddSingleton(guidGenerator);

        var hallAccessCardManager = new HallAccessCardManager(
            hallAccessCardRepository,
            cardNumberGenerator)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        return new PaymentManager(
            paymentRepository,
            reservationRepository,
            receiptNumberGenerator,
            CreateUnusedDepositConfirmationService(reservationRepository, services),
            hallAccessCardManager)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };
    }

    private static DepositConfirmationService CreateUnusedDepositConfirmationService(
        IReservationRepository reservationRepository,
        ServiceCollection services)
    {
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

        var schedulingManager = new ReservationSchedulingManager(reservationRepository)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        var hallAvailabilityManager = new HallAvailabilityManager
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };

        return new DepositConfirmationService(
            reservationRepository,
            hallRepository,
            hallAvailabilityManager,
            schedulingManager)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };
    }

    private static Reservation CreateConfirmedReservation(decimal totalPrice, decimal paidAmount)
    {
        return new Reservation(Guid.NewGuid())
        {
            HallId = HallId,
            CustomerId = Guid.NewGuid(),
            EventDate = Now.Date.AddDays(7),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = totalPrice,
            PaidAmount = paidAmount,
            Status = ReservationStatus.Confirmed,
        };
    }
}
