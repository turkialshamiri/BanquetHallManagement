using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Reports;

public interface IReportQueryExecutor
{
    Task<IReadOnlyList<HallPerformanceAggregate>> GetHallPerformanceAggregatesAsync(
        IQueryable<Reservation> filteredQuery);

    Task<IReadOnlyList<CustomerActivityAggregate>> GetCustomerActivityAggregatesAsync(
        IQueryable<Reservation> filteredQuery);
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
