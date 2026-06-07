using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reports;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Linq;

namespace BanquetHallManagement.EntityFrameworkCore.Reports;

public class EfCoreReportQueryExecutor :
    IReportQueryExecutor,
    ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public EfCoreReportQueryExecutor(IAsyncQueryableExecuter asyncExecuter)
    {
        _asyncExecuter = asyncExecuter;
    }

    public async Task<IReadOnlyList<HallPerformanceAggregate>> GetHallPerformanceAggregatesAsync(
        IQueryable<Reservation> filteredQuery)
    {
        return await _asyncExecuter.ToListAsync(
            filteredQuery
                .Where(r => r.Status != ReservationStatus.Cancelled)
                .Select(r => new HallPerformanceProjection
                {
                    HallId = r.HallId,
                    TotalPrice = r.TotalPrice,
                    GuestsCount = r.GuestsCount,
                    DurationMinutes = EF.Functions.DateDiffMinute(r.StartTime, r.EndTime),
                })
                .GroupBy(x => x.HallId)
                .Select(g => new HallPerformanceAggregate
                {
                    HallId = g.Key,
                    ReservationCount = g.Count(),
                    Revenue = g.Sum(x => x.TotalPrice),
                    AverageGuests = g.Average(x => (double)x.GuestsCount),
                    BookedHours = g.Sum(x => x.DurationMinutes) / 60.0,
                })
                .OrderByDescending(x => x.Revenue));
    }

    public async Task<IReadOnlyList<CustomerActivityAggregate>> GetCustomerActivityAggregatesAsync(
        IQueryable<Reservation> filteredQuery)
    {
        return await _asyncExecuter.ToListAsync(
            filteredQuery
                .Where(r => r.Status != ReservationStatus.Cancelled)
                .GroupBy(r => r.CustomerId)
                .Select(g => new CustomerActivityAggregate
                {
                    CustomerId = g.Key,
                    ReservationCount = g.Count(),
                    TotalSpent = g.Sum(x => x.TotalPrice),
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(20));
    }

    private sealed class HallPerformanceProjection
    {
        public Guid HallId { get; set; }

        public decimal TotalPrice { get; set; }

        public int GuestsCount { get; set; }

        public int DurationMinutes { get; set; }
    }
}
