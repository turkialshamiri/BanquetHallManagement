using Volo.Abp.Modularity;

namespace BanquetHallManagement;

[DependsOn(
    typeof(BanquetHallManagementApplicationModule),
    typeof(BanquetHallManagementDomainTestModule)
)]
public class BanquetHallManagementApplicationTestModule : AbpModule
{

}
