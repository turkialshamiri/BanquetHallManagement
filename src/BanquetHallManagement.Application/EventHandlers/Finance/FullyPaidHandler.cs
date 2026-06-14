using System;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class FullyPaidHandler :
    ILocalEventHandler<ReservationFullyPaidDomainEvent>,
    ITransientDependency
{
    private readonly ILogger<FullyPaidHandler> _logger;
    private readonly IReservationRepository _reservationRepository;
    private readonly IInvoiceManager _invoiceManager;
    private readonly IRepository<Payment, Guid> _paymentRepository;

    public FullyPaidHandler(
        ILogger<FullyPaidHandler> logger,
        IReservationRepository reservationRepository,
        IInvoiceManager invoiceManager,
        IRepository<Payment, Guid> paymentRepository)
    {
        _logger = logger;
        _reservationRepository = reservationRepository;
        _invoiceManager = invoiceManager;
        _paymentRepository = paymentRepository;
    }

    public async Task HandleEventAsync(ReservationFullyPaidDomainEvent eventData)
    {
        var snapshot = eventData.Snapshot;

        _logger.LogInformation(
            "Reservation fully paid | Reservation: {ReservationId} | PaidAmount: {PaidAmount} | TotalPrice: {TotalPrice}",
            snapshot.ReservationId,
            snapshot.PaidAmount,
            snapshot.TotalPrice);

        var reservation = await _reservationRepository.GetAsync(snapshot.ReservationId);
        var invoice = await _invoiceManager.EnsureFullyPaidInvoiceAsync(reservation);
        var payment = await _paymentRepository.GetAsync(invoice.PaymentId);

        await _paymentRepository.UpdateAsync(payment, autoSave: false);

        _logger.LogInformation(
            "Settlement invoice ensured | Invoice: {InvoiceNumber} ({InvoiceId}) | Reservation: {ReservationId}",
            invoice.InvoiceNumber,
            invoice.Id,
            reservation.Id);
    }
}
