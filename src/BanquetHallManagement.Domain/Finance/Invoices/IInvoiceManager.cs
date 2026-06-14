using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Finance.Invoices;

public interface IInvoiceManager
{
    Task<Invoice> CreateDepositInvoiceAsync(
        Payment payment,
        CancellationToken cancellationToken = default);

    Task<Invoice> EnsureFullyPaidInvoiceAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);
}
