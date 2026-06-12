using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using BanquetHallManagement.Reservations;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace BanquetHallManagement.Finance.Payments;

public class PaymentJournalEntryHandlerTests
{
    private static readonly DateTime Now = new(2026, 6, 10, 12, 0, 0);

    [Fact]
    public async Task HandleEventAsync_Should_Post_Deposit_Journal_Entry()
    {
        var reservation = CreateReservation(100_000m);
        var payment = CreatePayment(PaymentType.Deposit, 30_000m, reservation.Id);
        var handler = CreateHandler(payment, reservation, out var postingService);

        await handler.HandleEventAsync(CreateEvent(payment));

        await postingService.Received(1).PostDepositRevenueAsync(
            payment,
            reservation,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleEventAsync_Should_Post_Full_Deposit_Journal_Entry()
    {
        var reservation = CreateReservation(90_000m);
        var payment = CreatePayment(PaymentType.Deposit, 90_000m, reservation.Id);
        var handler = CreateHandler(payment, reservation, out var postingService);

        await handler.HandleEventAsync(CreateEvent(payment));

        await postingService.Received(1).PostFullDepositPaymentAsync(
            payment,
            reservation,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleEventAsync_Should_Post_Installment_Journal_Entry()
    {
        var reservation = CreateReservation(100_000m);
        var payment = CreatePayment(PaymentType.Installment, 20_000m, reservation.Id);
        var handler = CreateHandler(payment, reservation, out var postingService);

        await handler.HandleEventAsync(CreateEvent(payment));

        await postingService.Received(1).PostDeferredRevenueAsync(
            payment,
            reservation,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleEventAsync_Should_Post_Final_Payment_As_Deferred_Revenue()
    {
        var reservation = CreateReservation(100_000m);
        var payment = CreatePayment(PaymentType.Final, 20_000m, reservation.Id);
        var handler = CreateHandler(payment, reservation, out var postingService);

        await handler.HandleEventAsync(CreateEvent(payment));

        await postingService.Received(1).PostDeferredRevenueAsync(
            payment,
            reservation,
            Arg.Any<CancellationToken>());
    }

    private static PaymentJournalEntryHandler CreateHandler(
        Payment payment,
        Reservation reservation,
        out IJournalPostingService postingService)
    {
        var paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        paymentRepository.GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(payment));

        var reservationRepository = Substitute.For<IReservationRepository>();
        reservationRepository.GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(reservation));

        postingService = Substitute.For<IJournalPostingService>();

        postingService.PostDepositRevenueAsync(
                Arg.Any<Payment>(),
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(CreatePostedEntry(callInfo.Arg<Payment>())));

        postingService.PostFullDepositPaymentAsync(
                Arg.Any<Payment>(),
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(CreatePostedEntry(callInfo.Arg<Payment>())));

        postingService.PostDeferredRevenueAsync(
                Arg.Any<Payment>(),
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(CreatePostedEntry(callInfo.Arg<Payment>())));

        paymentRepository.UpdateAsync(
                Arg.Any<Payment>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Payment>()));

        return new PaymentJournalEntryHandler(
            paymentRepository,
            reservationRepository,
            postingService);
    }

    private static Reservation CreateReservation(decimal totalPrice)
    {
        var reservation = new Reservation(Guid.NewGuid())
        {
            HallId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            EventDate = Now.Date.AddDays(7),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            GuestsCount = 100,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Confirmed,
        };

        reservation.AssignReservationNumber("RES-2026-00001");

        return reservation;
    }

    private static Payment CreatePayment(
        PaymentType paymentType,
        decimal amount,
        Guid reservationId)
    {
        return new Payment(
            Guid.NewGuid(),
            reservationId,
            amount,
            Now,
            paymentType,
            "RC-2026-00001");
    }

    private static PaymentReceivedDomainEvent CreateEvent(Payment payment)
    {
        return new PaymentReceivedDomainEvent(
            payment.Id,
            payment.ReservationId,
            payment.Amount,
            payment.PaymentType,
            payment.PaymentDate,
            payment.ReceiptNumber);
    }

    private static JournalEntry CreatePostedEntry(Payment payment)
    {
        var entry = new JournalEntry(
            Guid.NewGuid(),
            "JE-2026-00001",
            payment.PaymentDate,
            JournalEntrySourceType.DepositRevenue,
            "Test entry",
            payment.ReservationId,
            payment.Id);

        entry.AddLine(Guid.NewGuid(), Guid.NewGuid(), payment.Amount, 0m);
        entry.AddLine(Guid.NewGuid(), Guid.NewGuid(), 0m, payment.Amount);
        entry.Post(Now);

        payment.LinkJournalEntry(entry.Id);

        return entry;
    }
}
