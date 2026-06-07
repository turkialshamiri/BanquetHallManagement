using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Dashboard;

public class DashboardAppService : BanquetHallManagementAppService, IDashboardAppService
{
    private readonly IRepository<Hall, System.Guid> _hallRepository;
    private readonly IRepository<Customer, System.Guid> _customerRepository;
    private readonly IRepository<Service, System.Guid> _serviceRepository;
    private readonly IRepository<Reservation, System.Guid> _reservationRepository;

    public DashboardAppService(
        IRepository<Hall, System.Guid> hallRepository,
        IRepository<Customer, System.Guid> customerRepository,
        IRepository<Service, System.Guid> serviceRepository,
        IRepository<Reservation, System.Guid> reservationRepository)
    {
        _hallRepository = hallRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalHalls = await _hallRepository.CountAsync();
        var totalCustomers = await _customerRepository.CountAsync();
        var totalServices = await _serviceRepository.CountAsync();

        var reservationQuery = await _reservationRepository.GetQueryableAsync();

        var reservationStats = await AsyncExecuter.FirstOrDefaultAsync(
            reservationQuery
                .GroupBy(_ => 1)
                .Select(g => new ReservationAggregateResult
                {
                    TotalReservations = g.Count(),
                    TotalRevenue = g
                        .Where(r => r.Status != ReservationStatus.Cancelled)
                        .Sum(r => r.TotalPrice),
                    PendingReservations = g.Count(r => r.Status == ReservationStatus.Pending),
                    ConfirmedReservations = g.Count(r => r.Status == ReservationStatus.Confirmed),
                    CancelledReservations = g.Count(r => r.Status == ReservationStatus.Cancelled),
                    CompletedReservations = g.Count(r => r.Status == ReservationStatus.Completed),
                }));

        return new DashboardStatsDto
        {
            TotalHalls = totalHalls,
            TotalCustomers = totalCustomers,
            TotalServices = totalServices,
            TotalReservations = reservationStats?.TotalReservations ?? 0,
            TotalRevenue = reservationStats?.TotalRevenue ?? 0,
            PendingReservations = reservationStats?.PendingReservations ?? 0,
            ConfirmedReservations = reservationStats?.ConfirmedReservations ?? 0,
            CancelledReservations = reservationStats?.CancelledReservations ?? 0,
            CompletedReservations = reservationStats?.CompletedReservations ?? 0,
        };
    }

    private sealed class ReservationAggregateResult
    {
        public long TotalReservations { get; set; }

        public decimal TotalRevenue { get; set; }

        public long PendingReservations { get; set; }

        public long ConfirmedReservations { get; set; }

        public long CancelledReservations { get; set; }

        public long CompletedReservations { get; set; }
    }
}
