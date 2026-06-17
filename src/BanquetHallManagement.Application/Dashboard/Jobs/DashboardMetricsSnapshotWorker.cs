using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Timing;

namespace BanquetHallManagement.Dashboard.Jobs;

public class DashboardMetricsSnapshotWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ILogger<DashboardMetricsSnapshotWorker> _logger;
    private readonly DashboardMetricsSnapshotOptions _options;

    public DashboardMetricsSnapshotWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DashboardMetricsSnapshotWorker> logger,
        IOptions<DashboardMetricsSnapshotOptions> options)
        : base(timer, serviceScopeFactory)
    {
        _logger = logger;
        _options = options.Value ?? new DashboardMetricsSnapshotOptions();

        var periodMinutes = Math.Max(1, _options.SnapshotPeriodMinutes);
        Timer.Period = periodMinutes * 60 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var generator = workerContext.ServiceProvider.GetRequiredService<DashboardMetricsSnapshotGenerator>();
            var repository = workerContext.ServiceProvider.GetRequiredService<IRepository<DashboardMetricsSnapshot, Guid>>();
            var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();

            var data = await generator.GenerateAsync(workerContext.CancellationToken);
            var now = clock.Now;

            var snapshot = await repository.FindAsync(
                DashboardMetricsSnapshotConsts.SingletonId,
                includeDetails: false,
                cancellationToken: workerContext.CancellationToken);

            if (snapshot == null)
            {
                snapshot = new DashboardMetricsSnapshot(DashboardMetricsSnapshotConsts.SingletonId);
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

                await repository.InsertAsync(snapshot, autoSave: true, cancellationToken: workerContext.CancellationToken);
            }
            else
            {
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

                await repository.UpdateAsync(snapshot, autoSave: true, cancellationToken: workerContext.CancellationToken);
            }

            _logger.LogInformation(
                "Dashboard snapshot updated in {ElapsedMs}ms (Reservations={TotalReservations}, Revenue={TotalRevenue}, Deferred={TotalDeferredRevenue})",
                sw.ElapsedMilliseconds,
                data.TotalReservations,
                data.TotalRevenue,
                data.TotalDeferredRevenue);
        }
        catch (Exception ex)
        {
            // Never crash the host process; next period will retry.
            _logger.LogError(ex, "Dashboard snapshot worker failed after {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
    }
}

