using System;
using System.Threading.Tasks;
using BanquetHallManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Dashboard;

[Authorize(BanquetHallManagementPermissions.Dashboard.Default)]
public class DashboardAppService : BanquetHallManagementAppService, IDashboardAppService
{
    private readonly IDashboardMetricsSnapshotRepository _snapshotRepository;
    private readonly DashboardMetricsSnapshotGenerator _snapshotGenerator;
    private readonly DashboardMetricsSnapshotOptions _snapshotOptions;

    public DashboardAppService(
        IDashboardMetricsSnapshotRepository snapshotRepository,
        DashboardMetricsSnapshotGenerator snapshotGenerator,
        IOptions<DashboardMetricsSnapshotOptions> snapshotOptions)
    {
        _snapshotRepository = snapshotRepository;
        _snapshotGenerator = snapshotGenerator;
        _snapshotOptions = snapshotOptions.Value ?? new DashboardMetricsSnapshotOptions();
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var canViewRevenue = await AuthorizationService.IsGrantedAsync(
            BanquetHallManagementPermissions.Dashboard.ViewRevenue);

        var snapshot = await _snapshotRepository.FindAsync(
            DashboardMetricsSnapshotConsts.SingletonId,
            includeDetails: false);
        var snapshotExisted = snapshot != null;

        var maxAgeMinutes = Math.Max(1, _snapshotOptions.MaxSnapshotAgeMinutes);
        var now = Clock.Now;
        var isSnapshotStale = snapshot == null ||
                              snapshot.SnapshotCreatedAt == DateTime.MinValue ||
                              (now - snapshot.SnapshotCreatedAt) > TimeSpan.FromMinutes(maxAgeMinutes);

        if (snapshot == null || isSnapshotStale)
        {
            var data = await _snapshotGenerator.GenerateAsync();

            snapshot ??= new DashboardMetricsSnapshot(DashboardMetricsSnapshotConsts.SingletonId);
            snapshot.Apply(
                data.TotalHalls,
                data.TotalCustomers,
                data.TotalServices,
                data.TotalReservations,
                data.PendingReservations,
                data.ConfirmedReservations,
                data.CancelledReservations,
                data.CompletedReservations,
                data.TotalRevenue,
                data.TotalDeferredRevenue,
                now);

            if (snapshotExisted)
            {
                await _snapshotRepository.UpdateAsync(snapshot, autoSave: true);
            }
            else
            {
                await _snapshotRepository.InsertAsync(snapshot, autoSave: true);
            }
        }

        return new DashboardStatsDto
        {
            TotalHalls = snapshot.TotalHalls,
            TotalCustomers = snapshot.TotalCustomers,
            TotalServices = snapshot.TotalServices,
            TotalReservations = snapshot.TotalReservations,
            TotalRevenue = canViewRevenue ? snapshot.TotalRevenue : 0m,
            TotalDeferredRevenue = canViewRevenue ? snapshot.TotalDeferredRevenue : 0m,
            PendingReservations = snapshot.PendingReservations,
            ConfirmedReservations = snapshot.ConfirmedReservations,
            CancelledReservations = snapshot.CancelledReservations,
            CompletedReservations = snapshot.CompletedReservations,
        };
    }
}
