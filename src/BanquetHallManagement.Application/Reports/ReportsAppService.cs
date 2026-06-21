using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;

namespace BanquetHallManagement.Reports;

[Authorize(BanquetHallManagementPermissions.Reports.Default)]
public class ReportsAppService : BanquetHallManagementAppService, IReportsAppService
{
    private const decimal OperatingHoursPerDay = 12m;

    private readonly IReportQueryExecutor _reportQueryExecutor;
    private readonly IAccountBalanceService _accountBalanceService;

    public ReportsAppService(
        IReportQueryExecutor reportQueryExecutor,
        IAccountBalanceService accountBalanceService)
    {
        _reportQueryExecutor = reportQueryExecutor;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<ReportsResultDto> GetAsync(GetReportsInput input)
    {
        input ??= new GetReportsInput();

        var (dateFrom, dateTo) = NormalizeDateRange(input);

        if (dateFrom > dateTo)
        {
            throw new UserFriendlyException(L["Validation:ReportDateRangeInvalid"]);
        }

        var filteredQuery = await _reportQueryExecutor.CreateFilteredReservationQueryAsync(
            dateFrom,
            dateTo,
            input.HallId,
            input.Status);

        var periodDays = Math.Max(1, (dateTo.Date - dateFrom.Date).Days + 1);
        var availableHoursPerHall = periodDays * OperatingHoursPerDay;

        var periodAggregate = await _reportQueryExecutor.GetPeriodStatisticsAsync(filteredQuery);

        var earnedRevenue = await _accountBalanceService.GetEarnedRevenueForPeriodAsync(
            dateFrom,
            dateTo);

        var revenueAnalytics = await BuildRevenueAnalyticsAsync();

        var hallAggregates = await _reportQueryExecutor.GetHallPerformanceAggregatesAsync(
            filteredQuery);

        var hallNames = await _reportQueryExecutor.GetHallNamesByIdsAsync(
            hallAggregates.Select(row => row.HallId).ToList());

        var hallPerformanceRows = hallAggregates
            .Select(row => new HallPerformanceRow
            {
                HallName = GetName(hallNames, row.HallId),
                ReservationCount = row.ReservationCount,
                Revenue = row.Revenue,
                AverageGuests = row.AverageGuests,
                BookedHours = row.BookedHours,
            })
            .ToList();

        var customerAggregates = await _reportQueryExecutor.GetCustomerActivityAggregatesAsync(
            filteredQuery);

        var customerNames = await _reportQueryExecutor.GetCustomerNamesByIdsAsync(
            customerAggregates.Select(row => row.CustomerId).ToList());

        var customerActivityRows = customerAggregates
            .Select(row => new CustomerActivityRow
            {
                CustomerName = GetName(customerNames, row.CustomerId),
                ReservationCount = row.ReservationCount,
                TotalSpent = row.TotalSpent,
            })
            .ToList();

        var monthlyRows = await _accountBalanceService.GetMonthlyEarnedRevenueAsync(
            dateFrom,
            dateTo);

        return new ReportsResultDto
        {
            Summary = new ReportSummaryDto
            {
                TotalReservations = periodAggregate.TotalReservations,
                TotalRevenue = earnedRevenue,
                AverageReservationValue = periodAggregate.CompletedCount > 0
                    ? Math.Round(
                        earnedRevenue / periodAggregate.CompletedCount,
                        2)
                    : 0,
                ActiveCustomers = periodAggregate.ActiveCustomers,
            },
            RevenueAnalytics = revenueAnalytics,
            ReservationStatistics = new ReservationStatisticsDto
            {
                PendingCount = periodAggregate.PendingCount,
                ConfirmedCount = periodAggregate.ConfirmedCount,
                CancelledCount = periodAggregate.CancelledCount,
                CompletedCount = periodAggregate.CompletedCount,
            },
            HallPerformance = hallPerformanceRows
                .Select(row => new HallPerformanceDto
                {
                    HallName = row.HallName,
                    ReservationCount = row.ReservationCount,
                    Revenue = row.Revenue,
                    AverageGuests = Math.Round((decimal)row.AverageGuests, 1),
                    OccupancyRate = CalculateOccupancyRate(row.BookedHours, availableHoursPerHall),
                })
                .ToList(),
            CustomerActivity = customerActivityRows
                .Select(row => new CustomerActivityDto
                {
                    CustomerName = row.CustomerName,
                    ReservationCount = row.ReservationCount,
                    TotalSpent = row.TotalSpent,
                })
                .ToList(),
            MonthlyReport = monthlyRows
                .Select(row => new MonthlyRevenueDto
                {
                    Year = row.Year,
                    Month = row.Month,
                    MonthLabel = FormatMonthLabel(row.Year, row.Month),
                    ReservationCount = row.RecognitionCount,
                    Revenue = row.Revenue,
                })
                .ToList(),
        };
    }

    private async Task<RevenueAnalyticsDto> BuildRevenueAnalyticsAsync()
    {
        var today = Clock.Now.Date;
        var yesterday = today.AddDays(-1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var previousMonthEnd = monthStart.AddDays(-1);
        var yearStart = new DateTime(today.Year, 1, 1);
        var previousYearStart = new DateTime(today.Year - 1, 1, 1);
        var previousYearEnd = new DateTime(today.Year - 1, 12, 31);

        var todayRevenue = await _accountBalanceService.GetEarnedRevenueForPeriodAsync(today, today);
        var yesterdayRevenue = await _accountBalanceService.GetEarnedRevenueForPeriodAsync(
            yesterday,
            yesterday);
        var thisMonthRevenue = await _accountBalanceService.GetEarnedRevenueForPeriodAsync(
            monthStart,
            today);
        var previousMonthRevenue = await _accountBalanceService.GetEarnedRevenueForPeriodAsync(
            previousMonthStart,
            previousMonthEnd);
        var thisYearRevenue = await _accountBalanceService.GetEarnedRevenueForPeriodAsync(
            yearStart,
            today);
        var previousYearRevenue = await _accountBalanceService.GetEarnedRevenueForPeriodAsync(
            previousYearStart,
            previousYearEnd);

        return new RevenueAnalyticsDto
        {
            Today = BuildRevenuePeriod(todayRevenue, yesterdayRevenue),
            ThisMonth = BuildRevenuePeriod(thisMonthRevenue, previousMonthRevenue),
            ThisYear = BuildRevenuePeriod(thisYearRevenue, previousYearRevenue),
        };
    }

    private static RevenuePeriodDto BuildRevenuePeriod(decimal current, decimal previous)
    {
        return new RevenuePeriodDto
        {
            Amount = current,
            ChangePercent = previous > 0
                ? Math.Round((current - previous) / previous * 100, 1)
                : null,
        };
    }

    private static (DateTime DateFrom, DateTime DateTo) NormalizeDateRange(GetReportsInput input)
    {
        var today = DateTime.Today;
        var dateFrom = input.DateFrom?.Date ?? new DateTime(today.Year, today.Month, 1);
        var dateTo = input.DateTo?.Date ?? today;

        return (dateFrom, dateTo);
    }

    private static string GetName(IReadOnlyDictionary<Guid, string> names, Guid id)
    {
        return names.TryGetValue(id, out var name) ? name : string.Empty;
    }

    private static decimal CalculateOccupancyRate(double bookedHours, decimal availableHours)
    {
        if (availableHours <= 0 || bookedHours <= 0)
        {
            return 0;
        }

        var rate = (decimal)bookedHours / availableHours * 100m;

        return Math.Min(100, Math.Round(rate, 1));
    }

    private static string FormatMonthLabel(int year, int month)
    {
        var culture = CultureInfo.GetCultureInfo("ar-SA");

        return new DateTime(year, month, 1).ToString("MMMM yyyy", culture);
    }

    private sealed class HallPerformanceRow
    {
        public string HallName { get; set; } = string.Empty;

        public long ReservationCount { get; set; }

        public decimal Revenue { get; set; }

        public double AverageGuests { get; set; }

        public double BookedHours { get; set; }
    }

    private sealed class CustomerActivityRow
    {
        public string CustomerName { get; set; } = string.Empty;

        public long ReservationCount { get; set; }

        public decimal TotalSpent { get; set; }
    }
}
