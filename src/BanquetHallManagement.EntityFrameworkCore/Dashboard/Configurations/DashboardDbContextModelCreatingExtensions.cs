using Microsoft.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Dashboard.Configurations;

public static class DashboardDbContextModelCreatingExtensions
{
    public static void ConfigureDashboard(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new DashboardMetricsSnapshotConfiguration());
    }
}
