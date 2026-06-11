using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.Invoices;

public interface IInvoiceRepository : IRepository<Invoice, Guid>
{
    Task<Invoice?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}
