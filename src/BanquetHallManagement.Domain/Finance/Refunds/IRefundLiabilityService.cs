using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Finance.Refunds;

public interface IRefundLiabilityService
{
    /// <summary>
    /// Moves paid installment amounts from deferred revenue to customer refund liability.
    /// Deposit revenue is not affected.
    /// </summary>
    Task<JournalEntry?> TransferInstallmentsToLiabilityAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pays out the outstanding refund liability in cash to the customer.
    /// </summary>
    Task<JournalEntry> ProcessRefundAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);
}
