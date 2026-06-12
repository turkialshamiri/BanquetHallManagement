using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IJournalPostingService
{
    Task<JournalEntry> PostDepositRevenueAsync(
        Payment payment,
        Reservation reservation,
        CancellationToken cancellationToken = default);

    Task<JournalEntry> PostFullDepositPaymentAsync(
        Payment payment,
        Reservation reservation,
        CancellationToken cancellationToken = default);

    Task<JournalEntry> PostDeferredRevenueAsync(
        Payment payment,
        Reservation reservation,
        CancellationToken cancellationToken = default);
}
