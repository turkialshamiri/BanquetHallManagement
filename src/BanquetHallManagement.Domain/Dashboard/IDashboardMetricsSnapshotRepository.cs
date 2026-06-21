using System;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Dashboard;

public interface IDashboardMetricsSnapshotRepository : IRepository<DashboardMetricsSnapshot, Guid>
{
}
