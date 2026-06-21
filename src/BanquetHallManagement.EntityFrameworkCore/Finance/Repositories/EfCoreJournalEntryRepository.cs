using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Repositories;

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

    public async Task<JournalEntry?> FindByReservationAndSourceTypeAsync(
        Guid reservationId,
        JournalEntrySourceType sourceType,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var trackedEntry = dbContext.ChangeTracker
            .Entries<JournalEntry>()
            .Select(entry => entry.Entity)
            .FirstOrDefault(entry =>
                entry.ReservationId == reservationId &&
                entry.SourceType == sourceType);

        if (trackedEntry != null)
        {
            if (trackedEntry.Lines.Count == 0)
            {
                await dbContext.Entry(trackedEntry)
                    .Collection(entry => entry.Lines)
                    .LoadAsync(cancellationToken);
            }

            return trackedEntry;
        }

        var query = await GetQueryableAsync();

        return await query
            .Include(entry => entry.Lines)
            .FirstOrDefaultAsync(
                entry => entry.ReservationId == reservationId && entry.SourceType == sourceType,
                cancellationToken);
    }

    public async Task<decimal> SumPostedBalanceForAccountAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var journalEntryQuery = await GetQueryableAsync();

        var balance = await AsyncExecuter.SumAsync(
            from entry in journalEntryQuery
            where entry.IsPosted
            from line in entry.Lines
            where line.AccountId == accountId
            select line.Credit - line.Debit,
            cancellationToken);

        return balance < 0 ? 0m : balance;
    }

    public async Task<decimal> SumPostedRevenueForPeriodAsync(
        DateTime from,
        DateTime to,
        IReadOnlyCollection<Guid> revenueAccountIds,
        CancellationToken cancellationToken = default)
    {
        if (revenueAccountIds.Count == 0)
        {
            return 0m;
        }

        var periodStart = from.Date;
        var periodEnd = to.Date.AddDays(1).AddTicks(-1);
        var journalEntryQuery = await GetQueryableAsync();

        var amount = await AsyncExecuter.SumAsync(
            from entry in journalEntryQuery
            where entry.IsPosted &&
                  entry.PostedTime >= periodStart &&
                  entry.PostedTime <= periodEnd
            from line in entry.Lines
            where revenueAccountIds.Contains(line.AccountId)
            select line.Credit - line.Debit,
            cancellationToken);

        return amount < 0 ? 0m : amount;
    }

    public async Task<IReadOnlyList<MonthlyEarnedRevenueResult>> GetMonthlyPostedRevenueAsync(
        DateTime from,
        DateTime to,
        IReadOnlyCollection<Guid> revenueAccountIds,
        CancellationToken cancellationToken = default)
    {
        if (revenueAccountIds.Count == 0)
        {
            return [];
        }

        var periodStart = from.Date;
        var periodEnd = to.Date.AddDays(1).AddTicks(-1);
        var journalEntryQuery = await GetQueryableAsync();

        return await AsyncExecuter.ToListAsync(
            from entry in journalEntryQuery
            where entry.IsPosted &&
                  entry.PostedTime >= periodStart &&
                  entry.PostedTime <= periodEnd
            from line in entry.Lines
            where revenueAccountIds.Contains(line.AccountId)
            group line by new
            {
                entry.PostedTime!.Value.Year,
                entry.PostedTime!.Value.Month,
            }
            into grouped
            orderby grouped.Key.Year, grouped.Key.Month
            select new MonthlyEarnedRevenueResult
            {
                Year = grouped.Key.Year,
                Month = grouped.Key.Month,
                Revenue = grouped.Sum(item => item.Credit - item.Debit) < 0
                    ? 0m
                    : grouped.Sum(item => item.Credit - item.Debit),
                RecognitionCount = grouped.Select(item => item.JournalEntryId).Distinct().Count(),
            },
            cancellationToken);
    }
}
