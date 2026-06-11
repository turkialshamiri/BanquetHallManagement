using System.Threading.Tasks;
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace BanquetHallManagement.Finance.Jobs;

public class ReservationPaymentMonitorJob : AsyncPeriodicBackgroundWorkerBase
{
    public ReservationPaymentMonitorJob(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = ReservationPaymentMonitorConsts.JobPeriodMinutes * 60 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var monitorService = workerContext
            .ServiceProvider
            .GetRequiredService<IReservationPaymentMonitorService>();

        await monitorService.ProcessAsync();
    }
}
