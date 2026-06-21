using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.JournalEntries;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Accounts;

public class AccountBalanceService : DomainService, IAccountBalanceService
{
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;

    public AccountBalanceService(
        IRepository<Account, Guid> accountRepository,
        IJournalEntryRepository journalEntryRepository)
    {
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
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
        var revenueAccountIds = await GetRevenueAccountIdsAsync(cancellationToken);

        if (revenueAccountIds.Count == 0)
        {
            return 0m;
        }

        return await _journalEntryRepository.SumPostedRevenueForPeriodAsync(
            from,
            to,
            revenueAccountIds,
            cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyEarnedRevenueResult>> GetMonthlyEarnedRevenueAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var revenueAccountIds = await GetRevenueAccountIdsAsync(cancellationToken);

        if (revenueAccountIds.Count == 0)
        {
            return [];
        }

        return await _journalEntryRepository.GetMonthlyPostedRevenueAsync(
            from,
            to,
            revenueAccountIds,
            cancellationToken);
    }

    private async Task<decimal> GetAccountCreditBalanceAsync(
        string accountCode,
        CancellationToken cancellationToken)
    {
        var accountQuery = await _accountRepository.GetQueryableAsync();

        var accountId = await AsyncExecuter.FirstOrDefaultAsync(
            accountQuery
                .Where(account => account.Code == accountCode && account.IsActive)
                .Select(account => (Guid?)account.Id),
            cancellationToken);

        if (!accountId.HasValue)
        {
            return 0m;
        }

        return await _journalEntryRepository.SumPostedBalanceForAccountAsync(
            accountId.Value,
            cancellationToken);
    }

    private async Task<IReadOnlyList<Guid>> GetRevenueAccountIdsAsync(
        CancellationToken cancellationToken)
    {
        var accountQuery = await _accountRepository.GetQueryableAsync();

        return await AsyncExecuter.ToListAsync(
            accountQuery
                .Where(account =>
                    account.IsActive &&
                    (account.Code == FinanceAccountCodes.HallRevenue ||
                     account.Code == FinanceAccountCodes.ServiceRevenue ||
                     account.Code == FinanceAccountCodes.NonRefundableDepositRevenue))
                .Select(account => account.Id),
            cancellationToken);
    }
}
