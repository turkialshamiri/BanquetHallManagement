using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IJournalEntryRepository : IRepository<JournalEntry, Guid>
{
    Task<JournalEntry?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<JournalEntry?> FindByReservationAndSourceTypeAsync(
        Guid reservationId,
        JournalEntrySourceType sourceType,
        CancellationToken cancellationToken = default);

    Task<decimal> SumPostedBalanceForAccountAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    Task<decimal> SumPostedRevenueForPeriodAsync(
        DateTime from,
        DateTime to,
        IReadOnlyCollection<Guid> revenueAccountIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyEarnedRevenueResult>> GetMonthlyPostedRevenueAsync(
        DateTime from,
        DateTime to,
        IReadOnlyCollection<Guid> revenueAccountIds,
        CancellationToken cancellationToken = default);
}
