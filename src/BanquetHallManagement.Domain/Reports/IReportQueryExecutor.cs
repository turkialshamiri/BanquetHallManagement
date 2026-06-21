using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Reports;

public interface IReportQueryExecutor
{
    Task<IQueryable<Reservation>> CreateFilteredReservationQueryAsync(
        DateTime dateFrom,
        DateTime dateTo,
        Guid? hallId,
        ReservationStatus? status,
        CancellationToken cancellationToken = default);

    Task<PeriodStatisticsAggregate> GetPeriodStatisticsAsync(
        IQueryable<Reservation> filteredQuery,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, string>> GetHallNamesByIdsAsync(
        IReadOnlyCollection<Guid> hallIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, string>> GetCustomerNamesByIdsAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HallPerformanceAggregate>> GetHallPerformanceAggregatesAsync(
        IQueryable<Reservation> filteredQuery);

    Task<IReadOnlyList<CustomerActivityAggregate>> GetCustomerActivityAggregatesAsync(
        IQueryable<Reservation> filteredQuery);
}

public class PeriodStatisticsAggregate
{
    public long TotalReservations { get; set; }

    public long ActiveCustomers { get; set; }

    public long PendingCount { get; set; }

    public long ConfirmedCount { get; set; }

    public long CancelledCount { get; set; }

    public long CompletedCount { get; set; }
}

public class HallPerformanceAggregate
{
    public Guid HallId { get; set; }

    public long ReservationCount { get; set; }

    public decimal Revenue { get; set; }

    public double AverageGuests { get; set; }

    public double BookedHours { get; set; }
}

public class CustomerActivityAggregate
{
    public Guid CustomerId { get; set; }

    public long ReservationCount { get; set; }

    public decimal TotalSpent { get; set; }
}
