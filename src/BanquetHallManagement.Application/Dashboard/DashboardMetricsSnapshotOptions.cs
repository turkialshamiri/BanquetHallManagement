namespace BanquetHallManagement.Dashboard;

public class DashboardMetricsSnapshotOptions
{
    public int SnapshotPeriodMinutes { get; set; } = 5;

    // If the snapshot is older than this threshold, the API will recompute live as a fallback.
    public int MaxSnapshotAgeMinutes { get; set; } = 10;
}

