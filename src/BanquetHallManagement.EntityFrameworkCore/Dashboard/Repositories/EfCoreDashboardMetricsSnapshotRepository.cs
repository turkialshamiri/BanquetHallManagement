using System;
using BanquetHallManagement.Dashboard;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Dashboard.Repositories;

public class EfCoreDashboardMetricsSnapshotRepository :
    EfCoreRepository<BanquetHallManagementDbContext, DashboardMetricsSnapshot, Guid>,
    IDashboardMetricsSnapshotRepository
{
    public EfCoreDashboardMetricsSnapshotRepository(
        IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
