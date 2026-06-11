using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.JournalEntries;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Repositories;

public class EfCoreJournalEntryRepository :
    EfCoreRepository<BanquetHallManagementDbContext, JournalEntry, Guid>,
    IJournalEntryRepository
{
    public EfCoreJournalEntryRepository(IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<JournalEntry>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).Include(x => x.Lines);
    }

    public async Task<JournalEntry?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var trackedEntry = dbContext.ChangeTracker
            .Entries<JournalEntry>()
            .Select(entry => entry.Entity)
            .FirstOrDefault(entry => entry.PaymentId == paymentId);

        if (trackedEntry != null)
        {
            return trackedEntry;
        }

        var query = await GetQueryableAsync();

        return await query.FirstOrDefaultAsync(
            entry => entry.PaymentId == paymentId,
            cancellationToken);
    }
}
