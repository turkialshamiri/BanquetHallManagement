using BanquetHallManagement.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.Dashboard;

public class DashboardMetricsSnapshotConfiguration : IEntityTypeConfiguration<DashboardMetricsSnapshot>
{
    public void Configure(EntityTypeBuilder<DashboardMetricsSnapshot> builder)
    {
        builder.ToTable("DashboardMetricsSnapshots");

        builder.ConfigureByConvention();

        builder.Property(x => x.TotalRevenue)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TotalDeferredRevenue)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.SnapshotCreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.SnapshotCreatedAt);
    }
}

