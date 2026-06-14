using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.Invoices;

public interface IInvoiceRepository : IRepository<Invoice, Guid>
{
    Task<Invoice?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<Invoice?> FindByReservationIdAndTypeAsync(
        Guid reservationId,
        InvoiceType invoiceType,
        CancellationToken cancellationToken = default);

    Task<Invoice?> FindSettlementInvoiceByReservationAsync(
        Guid reservationId,
        decimal totalPrice,
        CancellationToken cancellationToken = default);
}
