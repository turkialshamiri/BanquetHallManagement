using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Reports;

public class ReportsAppService : BanquetHallManagementAppService, IReportsAppService
{
    private const decimal OperatingHoursPerDay = 12m;

    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IReportQueryExecutor _reportQueryExecutor;

    public ReportsAppService(
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Customer, Guid> customerRepository,
        IReportQueryExecutor reportQueryExecutor)
    {
        _reservationRepository = reservationRepository;
        _hallRepository = hallRepository;
        _customerRepository = customerRepository;
        _reportQueryExecutor = reportQueryExecutor;
    }

    public async Task<ReportsResultDto> GetAsync(GetReportsInput input)
    {
        input ??= new GetReportsInput();

        var (dateFrom, dateTo) = NormalizeDateRange(input);

        if (dateFrom > dateTo)
        {
            throw new UserFriendlyException("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");
        }

        var reservationQuery = await _reservationRepository.GetQueryableAsync();
        var hallQuery = await _hallRepository.GetQueryableAsync();
        var customerQuery = await _customerRepository.GetQueryableAsync();

        var filteredQuery = ApplyFilters(reservationQuery, input, dateFrom, dateTo);
        var contextualQuery = ApplyContextualFilters(reservationQuery, input);

        var periodDays = Math.Max(1, (dateTo.Date - dateFrom.Date).Days + 1);
        var availableHoursPerHall = periodDays * OperatingHoursPerDay;

        var periodAggregate = await AsyncExecuter.FirstOrDefaultAsync(
            filteredQuery
                .GroupBy(_ => 1)
                .Select(g => new PeriodAggregateResult
                {
                    TotalReservations = g.Count(),
                    TotalRevenue = g
                        .Where(r => r.Status != ReservationStatus.Cancelled)
                        .Sum(r => r.TotalPrice),
                    RevenueReservationCount = g.Count(r => r.Status != ReservationStatus.Cancelled),
                    ActiveCustomers = g
                        .Where(r => r.Status != ReservationStatus.Cancelled)
                        .Select(r => r.CustomerId)
                        .Distinct()
                        .Count(),
                    PendingCount = g.Count(r => r.Status == ReservationStatus.Pending),
                    ConfirmedCount = g.Count(r => r.Status == ReservationStatus.Confirmed),
                    CancelledCount = g.Count(r => r.Status == ReservationStatus.Cancelled),
                    CompletedCount = g.Count(r => r.Status == ReservationStatus.Completed),
                }));

        periodAggregate ??= new PeriodAggregateResult();

        var revenueAnalytics = await BuildRevenueAnalyticsAsync(contextualQuery);

        var hallAggregates = await _reportQueryExecutor.GetHallPerformanceAggregatesAsync(
            filteredQuery);

        var hallIds = hallAggregates.Select(x => x.HallId).ToList();

        var hallNames = hallIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await AsyncExecuter.ToListAsync(
                hallQuery
                    .Where(h => hallIds.Contains(h.Id))
                    .Select(h => new { h.Id, h.Name })))
                .ToDictionary(x => x.Id, x => x.Name);

        var hallPerformanceRows = hallAggregates
            .Select(row => new HallPerformanceRow
            {
                HallName = hallNames.GetValueOrDefault(row.HallId, string.Empty),
                ReservationCount = row.ReservationCount,
                Revenue = row.Revenue,
                AverageGuests = row.AverageGuests,
                BookedHours = row.BookedHours,
            })
            .ToList();

        var customerAggregates = await _reportQueryExecutor.GetCustomerActivityAggregatesAsync(
            filteredQuery);

        var customerIds = customerAggregates.Select(x => x.CustomerId).ToList();

        var customerNames = customerIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await AsyncExecuter.ToListAsync(
                customerQuery
                    .Where(c => customerIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name })))
                .ToDictionary(x => x.Id, x => x.Name);

        var customerActivityRows = customerAggregates
            .Select(row => new CustomerActivityRow
            {
                CustomerName = customerNames.GetValueOrDefault(row.CustomerId, string.Empty),
                ReservationCount = row.ReservationCount,
                TotalSpent = row.TotalSpent,
            })
            .ToList();

        var monthlyRows = await AsyncExecuter.ToListAsync(
            filteredQuery
                .Where(r => r.Status != ReservationStatus.Cancelled)
                .GroupBy(r => new { r.EventDate.Year, r.EventDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyAggregateRow
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    ReservationCount = g.Count(),
                    Revenue = g.Sum(x => x.TotalPrice),
                }));

        return new ReportsResultDto
        {
            Summary = new ReportSummaryDto
            {
                TotalReservations = periodAggregate.TotalReservations,
                TotalRevenue = periodAggregate.TotalRevenue,
                AverageReservationValue = periodAggregate.RevenueReservationCount > 0
                    ? Math.Round(
                        periodAggregate.TotalRevenue / periodAggregate.RevenueReservationCount,
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
                    ReservationCount = row.ReservationCount,
                    Revenue = row.Revenue,
                })
                .ToList(),
        };
    }

    private async Task<RevenueAnalyticsDto> BuildRevenueAnalyticsAsync(
        IQueryable<Reservation> contextualQuery)
    {
        var today = Clock.Now.Date;
        var yesterday = today.AddDays(-1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var previousMonthEnd = monthStart.AddDays(-1);
        var yearStart = new DateTime(today.Year, 1, 1);
        var previousYearStart = new DateTime(today.Year - 1, 1, 1);
        var previousYearEnd = new DateTime(today.Year - 1, 12, 31);

        var revenueBase = contextualQuery.Where(r => r.Status != ReservationStatus.Cancelled);

        var revenueAggregate = await AsyncExecuter.FirstOrDefaultAsync(
            revenueBase
                .GroupBy(_ => 1)
                .Select(g => new RevenueAggregateResult
                {
                    Today = g.Where(r => r.EventDate.Date == today).Sum(r => r.TotalPrice),
                    Yesterday = g.Where(r => r.EventDate.Date == yesterday).Sum(r => r.TotalPrice),
                    ThisMonth = g.Where(r =>
                            r.EventDate >= monthStart &&
                            r.EventDate <= today)
                        .Sum(r => r.TotalPrice),
                    PreviousMonth = g.Where(r =>
                            r.EventDate >= previousMonthStart &&
                            r.EventDate <= previousMonthEnd)
                        .Sum(r => r.TotalPrice),
                    ThisYear = g.Where(r =>
                            r.EventDate >= yearStart &&
                            r.EventDate <= today)
                        .Sum(r => r.TotalPrice),
                    PreviousYear = g.Where(r =>
                            r.EventDate >= previousYearStart &&
                            r.EventDate <= previousYearEnd)
                        .Sum(r => r.TotalPrice),
                }));

        revenueAggregate ??= new RevenueAggregateResult();

        return new RevenueAnalyticsDto
        {
            Today = BuildRevenuePeriod(
                revenueAggregate.Today,
                revenueAggregate.Yesterday),
            ThisMonth = BuildRevenuePeriod(
                revenueAggregate.ThisMonth,
                revenueAggregate.PreviousMonth),
            ThisYear = BuildRevenuePeriod(
                revenueAggregate.ThisYear,
                revenueAggregate.PreviousYear),
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

    private static IQueryable<Reservation> ApplyFilters(
        IQueryable<Reservation> query,
        GetReportsInput input,
        DateTime dateFrom,
        DateTime dateTo)
    {
        query = ApplyContextualFilters(query, input);

        return query.Where(r =>
            r.EventDate.Date >= dateFrom &&
            r.EventDate.Date <= dateTo);
    }

    private static IQueryable<Reservation> ApplyContextualFilters(
        IQueryable<Reservation> query,
        GetReportsInput input)
    {
        if (input.HallId.HasValue)
        {
            query = query.Where(r => r.HallId == input.HallId.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(r => r.Status == input.Status.Value);
        }

        return query;
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

    private sealed class PeriodAggregateResult
    {
        public long TotalReservations { get; set; }

        public decimal TotalRevenue { get; set; }

        public long RevenueReservationCount { get; set; }

        public long ActiveCustomers { get; set; }

        public long PendingCount { get; set; }

        public long ConfirmedCount { get; set; }

        public long CancelledCount { get; set; }

        public long CompletedCount { get; set; }
    }

    private sealed class RevenueAggregateResult
    {
        public decimal Today { get; set; }

        public decimal Yesterday { get; set; }

        public decimal ThisMonth { get; set; }

        public decimal PreviousMonth { get; set; }

        public decimal ThisYear { get; set; }

        public decimal PreviousYear { get; set; }
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

    private sealed class MonthlyAggregateRow
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public long ReservationCount { get; set; }

        public decimal Revenue { get; set; }
    }
}
