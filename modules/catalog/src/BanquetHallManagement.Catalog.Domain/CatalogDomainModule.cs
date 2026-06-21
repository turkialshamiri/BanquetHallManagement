using BanquetHallManagement;
using Volo.Abp.Modularity;

namespace BanquetHallManagement.Catalog;

/// <summary>
/// Catalog bounded context (Halls, Customers, Services) — domain layer module.
/// </summary>
[DependsOn(typeof(CatalogDomainSharedModule))]
public class CatalogDomainModule : AbpModule
{
}
