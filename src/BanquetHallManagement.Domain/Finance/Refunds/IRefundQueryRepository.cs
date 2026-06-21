using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Finance.Refunds;

public interface IRefundQueryRepository
{
    Task<HashSet<Guid>> GetProcessedRefundReservationIdsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CancelledRefundCandidate>> GetCancelledRefundCandidatesAsync(
        string? textFilter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetPaymentsByReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<Account> GetRefundLiabilityAccountAsync(
        CancellationToken cancellationToken = default);
}

public class CancelledRefundCandidate
{
    public Reservation Reservation { get; init; } = null!;

    public string CustomerName { get; init; } = string.Empty;

    public string HallName { get; init; } = string.Empty;

    public IReadOnlyList<Payment> Payments { get; init; } = [];

    public JournalEntry? LiabilityEntry { get; init; }
}
