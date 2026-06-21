using BanquetHallManagement.Catalog;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace BanquetHallManagement;

[DependsOn(
    typeof(CatalogApplicationContractsModule),
    typeof(CatalogDomainModule),
    typeof(BanquetHallManagementDomainModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
)]
public class CatalogApplicationModule : AbpModule
{
}
