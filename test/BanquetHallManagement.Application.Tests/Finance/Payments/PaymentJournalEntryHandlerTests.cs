using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace BanquetHallManagement.Finance.Payments;

public class PaymentJournalEntryHandlerTests
{
    private static readonly DateTime Now = new(2026, 6, 10, 12, 0, 0);

    [Fact]
    public async Task HandleEventAsync_Should_Post_Deposit_Journal_Entry()
    {
        var payment = CreatePayment(PaymentType.Deposit);
        var handler = CreateHandler(payment, out var postingService);

        await handler.HandleEventAsync(CreateEvent(payment));

        await postingService.Received(1).PostDepositRevenueAsync(
            payment,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleEventAsync_Should_Post_Installment_Journal_Entry()
    {
        var payment = CreatePayment(PaymentType.Installment);
        var handler = CreateHandler(payment, out var postingService);

        await handler.HandleEventAsync(CreateEvent(payment));

        await postingService.Received(1).PostDeferredRevenueAsync(
            payment,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleEventAsync_Should_Throw_For_Unsupported_Payment_Type()
    {
        var payment = CreatePayment(PaymentType.Final);
        var handler = CreateHandler(payment, out _);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            handler.HandleEventAsync(CreateEvent(payment)));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.PaymentJournalPostingNotSupported);
    }

    private static PaymentJournalEntryHandler CreateHandler(
        Payment payment,
        out IJournalPostingService postingService)
    {
        var paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        paymentRepository.GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(payment));

        postingService = Substitute.For<IJournalPostingService>();

        postingService.PostDepositRevenueAsync(
                Arg.Any<Payment>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(CreatePostedEntry(callInfo.Arg<Payment>())));

        postingService.PostDeferredRevenueAsync(
                Arg.Any<Payment>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(CreatePostedEntry(callInfo.Arg<Payment>())));

        paymentRepository.UpdateAsync(
                Arg.Any<Payment>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Payment>()));

        return new PaymentJournalEntryHandler(paymentRepository, postingService);
    }

    private static Payment CreatePayment(PaymentType paymentType)
    {
        return new Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30_000m,
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
