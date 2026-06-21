using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace BanquetHallManagement.Finance.Invoices;

public class FullyPaidHandlerTests
{
    [Fact]
    public async Task HandleEventAsync_Should_Ensure_Settlement_Invoice()
    {
        var reservationId = Guid.NewGuid();
        var reservation = ReservationTestData.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date,
            TimeSpan.FromHours(18),
            TimeSpan.FromHours(22),
            totalPrice: 40_000m,
            paidAmount: 40_000m,
            id: reservationId);
        reservation.AssignReservationNumber("RSV-2026-00010");

        var invoice = new Invoice(
            Guid.NewGuid(),
            "INV-2026-00020",
            reservationId,
            Guid.NewGuid(),
            InvoiceType.Final,
            40_000m,
            DateTime.UtcNow);

        var payment = new Payment(
            invoice.PaymentId,
            reservationId,
            10_000m,
            DateTime.UtcNow,
            PaymentType.Installment,
            "RC-2026-00020");

        var reservationRepository = Substitute.For<IReservationRepository>();
        reservationRepository.GetAsync(reservationId).Returns(reservation);

        var invoiceManager = Substitute.For<IInvoiceManager>();
        invoiceManager.EnsureFullyPaidInvoiceAsync(
                reservation,
                Arg.Any<CancellationToken>())
            .Returns(invoice);

        var paymentRepository = Substitute.For<IRepository<Payment, Guid>>();
        paymentRepository.GetAsync(invoice.PaymentId).Returns(payment);

        var handler = new FullyPaidHandler(
            NullLogger<FullyPaidHandler>.Instance,
            reservationRepository,
            invoiceManager,
            paymentRepository);

        await handler.HandleEventAsync(
            new ReservationFullyPaidDomainEvent(
                ReservationEventSnapshot.FromReservation(reservation)));

        await invoiceManager.Received(1).EnsureFullyPaidInvoiceAsync(
            reservation,
            Arg.Any<CancellationToken>());

        await paymentRepository.Received(1).UpdateAsync(
            payment,
            Arg.Is(false),
            Arg.Any<CancellationToken>());
    }
}
