using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace BanquetHallManagement;

[DependsOn(
    typeof(CatalogDomainSharedModule),
    typeof(BanquetHallManagementDomainSharedModule),
    typeof(AbpDddApplicationContractsModule)
)]
public class CatalogApplicationContractsModule : AbpModule
{
}
