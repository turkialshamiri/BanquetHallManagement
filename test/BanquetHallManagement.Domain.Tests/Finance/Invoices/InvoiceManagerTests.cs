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
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
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
        var manager = CreateManager([], [], out var invoiceRepository);

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
        var manager = CreateManager([], [], out _);

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
        var manager = CreateManager([], [], out var invoiceRepository);

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

        var manager = CreateManager([], [], out _);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.CreateDepositInvoiceAsync(payment));

        exception.Code.ShouldBe(BanquetHallManagementDomainErrorCodes.InvoiceCreationNotSupported);
    }

    [Fact]
    public async Task EnsureFullyPaidInvoiceAsync_Should_Create_Final_Invoice_For_Total_Price()
    {
        var reservation = CreateFullyPaidReservation(50_000m, 50_000m);
        var payment = CreateDepositPayment(reservation.Id, 50_000m);
        var manager = CreateManager([], [payment], out var invoiceRepository);

        var invoice = await manager.EnsureFullyPaidInvoiceAsync(reservation);

        invoice.InvoiceType.ShouldBe(InvoiceType.Final);
        invoice.Amount.ShouldBe(reservation.TotalPrice);
        invoice.ReservationId.ShouldBe(reservation.Id);
        payment.InvoiceId.ShouldBe(invoice.Id);

        await invoiceRepository.Received(1).InsertAsync(
            Arg.Any<Invoice>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureFullyPaidInvoiceAsync_Should_Return_Existing_Final_Invoice()
    {
        var reservation = CreateFullyPaidReservation(50_000m, 50_000m);
        var existingInvoice = new Invoice(
            Guid.NewGuid(),
            "INV-2026-00009",
            reservation.Id,
            Guid.NewGuid(),
            InvoiceType.Final,
            reservation.TotalPrice,
            Now);

        var manager = CreateManager([existingInvoice], [], out var invoiceRepository);

        var invoice = await manager.EnsureFullyPaidInvoiceAsync(reservation);

        invoice.Id.ShouldBe(existingInvoice.Id);

        await invoiceRepository.DidNotReceive().InsertAsync(
            Arg.Any<Invoice>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureFullyPaidInvoiceAsync_Should_Reuse_Full_Deposit_Invoice()
    {
        var reservation = CreateFullyPaidReservation(50_000m, 50_000m);
        var paymentId = Guid.NewGuid();
        var existingDepositInvoice = new Invoice(
            Guid.NewGuid(),
            "INV-2026-00002",
            reservation.Id,
            paymentId,
            InvoiceType.Deposit,
            reservation.TotalPrice,
            Now);

        var manager = CreateManager([existingDepositInvoice], [], out var invoiceRepository);

        var invoice = await manager.EnsureFullyPaidInvoiceAsync(reservation);

        invoice.Id.ShouldBe(existingDepositInvoice.Id);

        await invoiceRepository.DidNotReceive().InsertAsync(
            Arg.Any<Invoice>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
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
        IList<Payment> payments,
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

        invoiceRepository.FindByReservationIdAndTypeAsync(
                Arg.Any<Guid>(),
                Arg.Any<InvoiceType>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var reservationId = callInfo.Arg<Guid>();
                var invoiceType = callInfo.Arg<InvoiceType>();
                return Task.FromResult(
                    invoices.FirstOrDefault(invoice =>
                        invoice.ReservationId == reservationId &&
                        invoice.InvoiceType == invoiceType));
            });

        invoiceRepository.FindSettlementInvoiceByReservationAsync(
                Arg.Any<Guid>(),
                Arg.Any<decimal>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var reservationId = callInfo.Arg<Guid>();
                var totalPrice = callInfo.Arg<decimal>();
                return Task.FromResult(
                    invoices
                        .Where(invoice => invoice.ReservationId == reservationId && invoice.Amount >= totalPrice)
                        .OrderByDescending(invoice => invoice.IssuedAt)
                        .FirstOrDefault());
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

        var paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        paymentRepository
            .GetListAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Payment, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<System.Linq.Expressions.Expression<Func<Payment, bool>>>();
                var compiled = predicate.Compile();
                return Task.FromResult(payments.Where(compiled).ToList());
            });

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);

        var services = new ServiceCollection();
        services.AddSingleton(clock);
        services.AddSingleton(guidGenerator);

        return new InvoiceManager(invoiceRepository, invoiceNumberGenerator, paymentRepository)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider()),
        };
    }

    private static Reservation CreateFullyPaidReservation(decimal totalPrice, decimal paidAmount)
    {
        var reservation = new Reservation(Guid.NewGuid())
        {
            TotalPrice = totalPrice,
            PaidAmount = paidAmount,
        };

        reservation.AssignReservationNumber("RSV-2026-00001");
        return reservation;
    }

    private static Payment CreateDepositPayment(Guid reservationId, decimal amount)
    {
        return new Payment(
            Guid.NewGuid(),
            reservationId,
            amount,
            Now,
            PaymentType.Deposit,
            "RC-2026-00001");
    }

    private static Payment CreateDepositPayment()
    {
        return CreateDepositPayment(Guid.NewGuid(), 30_000m);
    }
}
