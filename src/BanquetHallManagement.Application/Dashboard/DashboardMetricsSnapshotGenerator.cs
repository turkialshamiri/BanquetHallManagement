using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace BanquetHallManagement.Dashboard;

public class DashboardMetricsSnapshotGenerator : ITransientDependency
{
    private readonly IRepository<Hall, System.Guid> _hallRepository;
    private readonly IRepository<Customer, System.Guid> _customerRepository;
    private readonly IRepository<Service, System.Guid> _serviceRepository;
    private readonly IRepository<Reservation, System.Guid> _reservationRepository;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public DashboardMetricsSnapshotGenerator(
        IRepository<Hall, System.Guid> hallRepository,
        IRepository<Customer, System.Guid> customerRepository,
        IRepository<Service, System.Guid> serviceRepository,
        IRepository<Reservation, System.Guid> reservationRepository,
        IAccountBalanceService accountBalanceService,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _hallRepository = hallRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _reservationRepository = reservationRepository;
        _accountBalanceService = accountBalanceService;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<DashboardMetricsSnapshotData> GenerateAsync(CancellationToken cancellationToken = default)
    {
        // Business logic intentionally mirrors DashboardAppService.GetStatsAsync().
        var totalHalls = await _hallRepository.CountAsync(cancellationToken);
        var totalCustomers = await _customerRepository.CountAsync(cancellationToken);
        var totalServices = await _serviceRepository.CountAsync(cancellationToken);

        var reservationQuery = await _reservationRepository.GetQueryableAsync();
        reservationQuery = reservationQuery.WhereActive();

        var reservationStats = await _asyncExecuter.FirstOrDefaultAsync(
            reservationQuery
                .GroupBy(_ => 1)
                .Select(g => new ReservationAggregateResult
                {
                    TotalReservations = g.Count(),
                    PendingReservations = g.Count(r => r.Status == ReservationStatus.Pending),
                    ConfirmedReservations = g.Count(r => r.Status == ReservationStatus.Confirmed),
                    CancelledReservations = g.Count(r => r.Status == ReservationStatus.Cancelled),
                    CompletedReservations = g.Count(r => r.Status == ReservationStatus.Completed),
                }),
            cancellationToken);

        var totalEarnedRevenue = await _accountBalanceService.GetEarnedRevenueBalanceAsync(cancellationToken);
        var totalDeferredRevenue = await _accountBalanceService.GetDeferredRevenueBalanceAsync(cancellationToken);

        reservationStats ??= new ReservationAggregateResult();

        return new DashboardMetricsSnapshotData
        {
            TotalHalls = totalHalls,
            TotalCustomers = totalCustomers,
            TotalServices = totalServices,
            TotalReservations = reservationStats.TotalReservations,
            TotalRevenue = totalEarnedRevenue,
            TotalDeferredRevenue = totalDeferredRevenue,
            PendingReservations = reservationStats.PendingReservations,
            ConfirmedReservations = reservationStats.ConfirmedReservations,
            CancelledReservations = reservationStats.CancelledReservations,
            CompletedReservations = reservationStats.CompletedReservations,
        };
    }

    private sealed class ReservationAggregateResult
    {
        public long TotalReservations { get; set; }
        public long PendingReservations { get; set; }
        public long ConfirmedReservations { get; set; }
        public long CancelledReservations { get; set; }
        public long CompletedReservations { get; set; }
    }
}

public sealed class DashboardMetricsSnapshotData
{
    public long TotalHalls { get; set; }
    public long TotalCustomers { get; set; }
    public long TotalServices { get; set; }
    public long TotalReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDeferredRevenue { get; set; }
    public long PendingReservations { get; set; }
    public long ConfirmedReservations { get; set; }
    public long CancelledReservations { get; set; }
    public long CompletedReservations { get; set; }
}

