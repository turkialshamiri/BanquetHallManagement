using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.Invoices.Events;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace BanquetHallManagement.Finance.Invoices;

public class InvoiceManagerTests
{
    private static readonly DateTime Now = new(2026, 6, 11, 12, 0, 0);

    [Fact]
    public async Task CreateDepositInvoiceAsync_Should_Create_Invoice_Linked_To_Payment_And_Reservation()
    {
        var payment = CreateDepositPayment();
        var manager = CreateManager([], out var invoiceRepository);

        var invoice = await manager.CreateDepositInvoiceAsync(payment);

        invoice.InvoiceType.ShouldBe(InvoiceType.Deposit);
        invoice.ReservationId.ShouldBe(payment.ReservationId);
        invoice.PaymentId.ShouldBe(payment.Id);
        invoice.Amount.ShouldBe(payment.Amount);
        invoice.InvoiceNumber.ShouldBe("INV-2026-00001");
        payment.InvoiceId.ShouldBe(invoice.Id);

        await invoiceRepository.Received(1).InsertAsync(
            Arg.Any<Invoice>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDepositInvoiceAsync_Should_Publish_DepositInvoiceCreatedEvent()
    {
        var payment = CreateDepositPayment();
        var manager = CreateManager([], out _);

        var invoice = await manager.CreateDepositInvoiceAsync(payment);

        var domainEvent = invoice.GetLocalEvents()
            .Select(record => record.EventData)
            .OfType<DepositInvoiceCreatedEvent>()
            .Single();

        domainEvent.InvoiceId.ShouldBe(invoice.Id);
        domainEvent.PaymentId.ShouldBe(payment.Id);
        domainEvent.ReservationId.ShouldBe(payment.ReservationId);
        domainEvent.Amount.ShouldBe(payment.Amount);
    }

    [Fact]
    public async Task CreateDepositInvoiceAsync_Should_Not_Create_Duplicate_Invoice_On_Retry()
    {
        var payment = CreateDepositPayment();
        var manager = CreateManager([], out var invoiceRepository);

        var firstInvoice = await manager.CreateDepositInvoiceAsync(payment);
        var secondInvoice = await manager.CreateDepositInvoiceAsync(payment);

        secondInvoice.Id.ShouldBe(firstInvoice.Id);

        await invoiceRepository.Received(1).InsertAsync(
            Arg.Any<Invoice>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDepositInvoiceAsync_Should_Reject_Non_Deposit_Payment()
    {
        var payment = CreateDepositPayment();
        payment = new Payment(
            payment.Id,
            payment.ReservationId,
            payment.Amount,
            payment.PaymentDate,
            PaymentType.Installment,
            payment.ReceiptNumber);

        var manager = CreateManager([], out _);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.CreateDepositInvoiceAsync(payment));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.InvoiceCreationNotSupported);
    }

    [Fact]
    public void LinkInvoice_Should_Throw_When_Different_Invoice_Already_Linked()
    {
        var payment = CreateDepositPayment();
        payment.LinkInvoice(Guid.NewGuid());

        var exception = Should.Throw<BusinessException>(() =>
            payment.LinkInvoice(Guid.NewGuid()));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.InvoiceDuplicateCreation);
    }

    private static InvoiceManager CreateManager(
        IList<Invoice> invoices,
        out IInvoiceRepository invoiceRepository)
    {
        invoiceRepository = Substitute.For<IInvoiceRepository>();

        invoiceRepository.FindAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                return Task.FromResult(invoices.FirstOrDefault(invoice => invoice.Id == id));
            });

        invoiceRepository.FindByPaymentIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var paymentId = callInfo.Arg<Guid>();
                return Task.FromResult(
                    invoices.FirstOrDefault(invoice => invoice.PaymentId == paymentId));
            });

        invoiceRepository.InsertAsync(
                Arg.Any<Invoice>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var invoice = callInfo.Arg<Invoice>();
                invoices.Add(invoice);
                return Task.FromResult(invoice);
            });

        var invoiceNumberGenerator = Substitute.For<IInvoiceNumberGenerator>();
        invoiceNumberGenerator.GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-2026-00001");

        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var services = new ServiceCollection();
        services.AddSingleton(clock);
        services.AddSingleton(guidGenerator);

        return new InvoiceManager(invoiceRepository, invoiceNumberGenerator)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };
    }

    private static Payment CreateDepositPayment()
    {
        return new Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30_000m,
            Now,
            PaymentType.Deposit,
            "RC-2026-00001");
    }
}
