using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Payments;

namespace BanquetHallManagement.Finance.Invoices;

public interface IInvoiceManager
{
    Task<Invoice> CreateDepositInvoiceAsync(
        Payment payment,
        CancellationToken cancellationToken = default);
}
