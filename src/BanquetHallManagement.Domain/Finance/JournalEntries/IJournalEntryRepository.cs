using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IJournalEntryRepository : IRepository<JournalEntry, Guid>
{
    Task<JournalEntry?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}
