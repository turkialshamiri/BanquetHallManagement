using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace BanquetHallManagement;

[DependsOn(typeof(AbpValidationModule))]
public class CatalogDomainSharedModule : AbpModule
{
}
