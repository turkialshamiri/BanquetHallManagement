using Volo.Abp.Modularity;

namespace BanquetHallManagement;

[DependsOn(
    typeof(BanquetHallManagementDomainModule),
    typeof(BanquetHallManagementTestBaseModule)
)]
public class BanquetHallManagementDomainTestModule : AbpModule
{

}
