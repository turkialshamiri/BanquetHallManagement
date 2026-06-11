using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using NSubstitute;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace BanquetHallManagement.Finance.Invoices;

public class CreateDepositInvoiceHandlerTests
{
    private static readonly DateTime Now = new(2026, 6, 11, 12, 0, 0);

    [Fact]
    public async Task HandleEventAsync_Should_Create_Deposit_Invoice()
    {
        var payment = CreatePayment(PaymentType.Deposit);
        var handler = CreateHandler(payment, out var invoiceManager);

        await handler.HandleEventAsync(CreateEvent(payment));

        await invoiceManager.Received(1).CreateDepositInvoiceAsync(
            payment,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleEventAsync_Should_Ignore_Installment_Payments()
    {
        var payment = CreatePayment(PaymentType.Installment);
        var handler = CreateHandler(payment, out var invoiceManager);

        await handler.HandleEventAsync(CreateEvent(payment));

        await invoiceManager.DidNotReceive().CreateDepositInvoiceAsync(
            Arg.Any<Payment>(),
            Arg.Any<CancellationToken>());
    }

    private static CreateDepositInvoiceHandler CreateHandler(
        Payment payment,
        out IInvoiceManager invoiceManager)
    {
        var paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        paymentRepository.GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(payment));

        paymentRepository.UpdateAsync(
                Arg.Any<Payment>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Payment>()));

        invoiceManager = Substitute.For<IInvoiceManager>();

        invoiceManager.CreateDepositInvoiceAsync(
                Arg.Any<Payment>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var targetPayment = callInfo.Arg<Payment>();
                var invoice = new Invoice(
                    Guid.NewGuid(),
                    "INV-2026-00001",
                    targetPayment.ReservationId,
                    targetPayment.Id,
                    InvoiceType.Deposit,
                    targetPayment.Amount,
                    targetPayment.PaymentDate);

                targetPayment.LinkInvoice(invoice.Id);
                return Task.FromResult(invoice);
            });

        return new CreateDepositInvoiceHandler(paymentRepository, invoiceManager);
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
}
