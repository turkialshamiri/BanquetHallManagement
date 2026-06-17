using System;
using Volo.Abp.Domain.Entities;

namespace BanquetHallManagement.Dashboard;

public class DashboardMetricsSnapshot : Entity<Guid>
{
    public long TotalHalls { get; private set; }
    public long TotalCustomers { get; private set; }
    public long TotalServices { get; private set; }
    public long TotalReservations { get; private set; }

    public long PendingReservations { get; private set; }
    public long ConfirmedReservations { get; private set; }
    public long CancelledReservations { get; private set; }
    public long CompletedReservations { get; private set; }

    public decimal TotalRevenue { get; private set; }
    public decimal TotalDeferredRevenue { get; private set; }

    public DateTime SnapshotCreatedAt { get; private set; }

    private DashboardMetricsSnapshot()
    {
    }

    public DashboardMetricsSnapshot(Guid id)
        : base(id)
    {
        SnapshotCreatedAt = DateTime.MinValue;
    }

    public void Apply(
        long totalHalls,
        long totalCustomers,
        long totalServices,
        long totalReservations,
        long pendingReservations,
        long confirmedReservations,
        long cancelledReservations,
        long completedReservations,
        decimal totalRevenue,
        decimal totalDeferredRevenue,
        DateTime snapshotCreatedAt)
    {
        TotalHalls = totalHalls;
        TotalCustomers = totalCustomers;
        TotalServices = totalServices;
        TotalReservations = totalReservations;

        PendingReservations = pendingReservations;
        ConfirmedReservations = confirmedReservations;
        CancelledReservations = cancelledReservations;
        CompletedReservations = completedReservations;

        TotalRevenue = totalRevenue;
        TotalDeferredRevenue = totalDeferredRevenue;
        SnapshotCreatedAt = snapshotCreatedAt;
    }
}

