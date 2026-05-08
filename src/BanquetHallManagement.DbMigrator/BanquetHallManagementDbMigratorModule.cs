using BanquetHallManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace BanquetHallManagement.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(BanquetHallManagementEntityFrameworkCoreModule),
    typeof(BanquetHallManagementApplicationContractsModule)
)]
public class BanquetHallManagementDbMigratorModule : AbpModule
{
}
