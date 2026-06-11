using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Invoices;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Repositories;

public class EfCoreInvoiceRepository :
    EfCoreRepository<BanquetHallManagementDbContext, Invoice, Guid>,
    IInvoiceRepository
{
    public EfCoreInvoiceRepository(IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Invoice?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var trackedInvoice = dbContext.ChangeTracker
            .Entries<Invoice>()
            .Select(entry => entry.Entity)
            .FirstOrDefault(invoice => invoice.PaymentId == paymentId);

        if (trackedInvoice != null)
        {
            return trackedInvoice;
        }

        var query = await GetQueryableAsync();

        return await query.FirstOrDefaultAsync(
            invoice => invoice.PaymentId == paymentId,
            cancellationToken);
    }
}
