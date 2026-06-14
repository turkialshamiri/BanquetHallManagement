using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BanquetHallManagement.Finance.Accounts;

public interface IAccountBalanceService
{
    Task<decimal> GetDeferredRevenueBalanceAsync(CancellationToken cancellationToken = default);

    Task<decimal> GetEarnedRevenueBalanceAsync(CancellationToken cancellationToken = default);

    Task<decimal> GetEarnedRevenueForPeriodAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyEarnedRevenueResult>> GetMonthlyEarnedRevenueAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}

public class MonthlyEarnedRevenueResult
{
    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Revenue { get; set; }

    public int RecognitionCount { get; set; }
}
