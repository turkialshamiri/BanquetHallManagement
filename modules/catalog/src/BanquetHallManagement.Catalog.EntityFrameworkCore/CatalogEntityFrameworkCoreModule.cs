using Volo.Abp.EntityFrameworkCore;
using BanquetHallManagement.Catalog;
using Volo.Abp.Modularity;

namespace BanquetHallManagement.EntityFrameworkCore;

[DependsOn(
    typeof(CatalogDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class CatalogEntityFrameworkCoreModule : AbpModule
{
}
