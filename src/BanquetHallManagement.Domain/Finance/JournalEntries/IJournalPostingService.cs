using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Payments;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IJournalPostingService
{
    Task<JournalEntry> PostDepositRevenueAsync(
        Payment payment,
        CancellationToken cancellationToken = default);

    Task<JournalEntry> PostDeferredRevenueAsync(
        Payment payment,
        CancellationToken cancellationToken = default);
}
