using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.JournalEntries;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Linq;

namespace BanquetHallManagement.Finance.Accounts;

public class AccountBalanceService : DomainService, IAccountBalanceService
{
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IRepository<JournalEntry, Guid> _journalEntryRepository;
    private readonly IRepository<JournalEntryLine, Guid> _journalEntryLineRepository;

    public AccountBalanceService(
        IRepository<Account, Guid> accountRepository,
        IRepository<JournalEntry, Guid> journalEntryRepository,
        IRepository<JournalEntryLine, Guid> journalEntryLineRepository)
    {
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _journalEntryLineRepository = journalEntryLineRepository;
    }

    public Task<decimal> GetDeferredRevenueBalanceAsync(CancellationToken cancellationToken = default)
    {
        return GetAccountCreditBalanceAsync(FinanceAccountCodes.DeferredRevenue, cancellationToken);
    }

    public async Task<decimal> GetEarnedRevenueBalanceAsync(CancellationToken cancellationToken = default)
    {
        var hallRevenue = await GetAccountCreditBalanceAsync(
            FinanceAccountCodes.HallRevenue,
            cancellationToken);
        var serviceRevenue = await GetAccountCreditBalanceAsync(
            FinanceAccountCodes.ServiceRevenue,
            cancellationToken);
        var depositRevenue = await GetAccountCreditBalanceAsync(
            FinanceAccountCodes.NonRefundableDepositRevenue,
            cancellationToken);

        return hallRevenue + serviceRevenue + depositRevenue;
    }

    public async Task<decimal> GetEarnedRevenueForPeriodAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var accountQuery = await _accountRepository.GetQueryableAsync();
        var journalEntryQuery = await _journalEntryRepository.GetQueryableAsync();
        var journalEntryLineQuery = await _journalEntryLineRepository.GetQueryableAsync();

        var revenueAccountIds = await AsyncExecuter.ToListAsync(
            accountQuery
                .Where(account =>
                    account.IsActive &&
                    (account.Code == FinanceAccountCodes.HallRevenue ||
                     account.Code == FinanceAccountCodes.ServiceRevenue ||
                     account.Code == FinanceAccountCodes.NonRefundableDepositRevenue))
                .Select(account => account.Id),
            cancellationToken);

        if (revenueAccountIds.Count == 0)
        {
            return 0m;
        }

        var periodStart = from.Date;
        var periodEnd = to.Date.AddDays(1).AddTicks(-1);

        var amount = await AsyncExecuter.SumAsync(
            from line in journalEntryLineQuery
            join entry in journalEntryQuery on line.JournalEntryId equals entry.Id
            where revenueAccountIds.Contains(line.AccountId) &&
                  entry.IsPosted &&
                  entry.PostedTime >= periodStart &&
                  entry.PostedTime <= periodEnd
            select line.Credit - line.Debit,
            cancellationToken);

        return amount < 0 ? 0m : amount;
    }

    public async Task<IReadOnlyList<MonthlyEarnedRevenueResult>> GetMonthlyEarnedRevenueAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var accountQuery = await _accountRepository.GetQueryableAsync();
        var journalEntryQuery = await _journalEntryRepository.GetQueryableAsync();
        var journalEntryLineQuery = await _journalEntryLineRepository.GetQueryableAsync();

        var revenueAccountIds = await AsyncExecuter.ToListAsync(
            accountQuery
                .Where(account =>
                    account.IsActive &&
                    (account.Code == FinanceAccountCodes.HallRevenue ||
                     account.Code == FinanceAccountCodes.ServiceRevenue ||
                     account.Code == FinanceAccountCodes.NonRefundableDepositRevenue))
                .Select(account => account.Id),
            cancellationToken);

        if (revenueAccountIds.Count == 0)
        {
            return [];
        }

        var periodStart = from.Date;
        var periodEnd = to.Date.AddDays(1).AddTicks(-1);

        return await AsyncExecuter.ToListAsync(
            from line in journalEntryLineQuery
            join entry in journalEntryQuery on line.JournalEntryId equals entry.Id
            where revenueAccountIds.Contains(line.AccountId) &&
                  entry.IsPosted &&
                  entry.PostedTime >= periodStart &&
                  entry.PostedTime <= periodEnd
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

    private async Task<decimal> GetAccountCreditBalanceAsync(
        string accountCode,
        CancellationToken cancellationToken)
    {
        var accountQuery = await _accountRepository.GetQueryableAsync();
        var journalEntryQuery = await _journalEntryRepository.GetQueryableAsync();
        var journalEntryLineQuery = await _journalEntryLineRepository.GetQueryableAsync();

        var accountId = await AsyncExecuter.FirstOrDefaultAsync(
            accountQuery
                .Where(account => account.Code == accountCode && account.IsActive)
                .Select(account => (Guid?)account.Id),
            cancellationToken);

        if (!accountId.HasValue)
        {
            return 0m;
        }

        var balance = await AsyncExecuter.SumAsync(
            from line in journalEntryLineQuery
            join entry in journalEntryQuery on line.JournalEntryId equals entry.Id
            where line.AccountId == accountId.Value && entry.IsPosted
            select line.Credit - line.Debit,
            cancellationToken);

        return balance < 0 ? 0m : balance;
    }
}
