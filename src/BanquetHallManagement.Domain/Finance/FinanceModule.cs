using BanquetHallManagement;
using Volo.Abp.Modularity;

namespace BanquetHallManagement.Finance;

/// <summary>
/// Finance bounded context — domain layer module.
/// </summary>
[DependsOn(typeof(BanquetHallManagementDomainSharedModule))]
public class FinanceDomainModule : AbpModule
{
}
