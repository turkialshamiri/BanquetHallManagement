using System.Threading.Tasks;
using BanquetHallManagement.Dashboard;
using BanquetHallManagement.Finance.Jobs;
using BanquetHallManagement.Dashboard.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;

namespace BanquetHallManagement;

[DependsOn(
    typeof(CatalogApplicationModule),
    typeof(BanquetHallManagementDomainModule),
    typeof(BanquetHallManagementApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class BanquetHallManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<DashboardMetricsSnapshotOptions>(options =>
        {
            configuration.GetSection("Dashboard:Snapshot").Bind(options);
        });
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<ReservationPaymentMonitorJob>();
        await context.AddBackgroundWorkerAsync<DashboardMetricsSnapshotWorker>();
    }
}

