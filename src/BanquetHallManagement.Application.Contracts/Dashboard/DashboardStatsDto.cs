namespace BanquetHallManagement.Dashboard;

public class DashboardStatsDto
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
