using Volo.Abp.Modularity;

namespace BanquetHallManagement;

/* Inherit from this class for your domain layer tests. */
public abstract class BanquetHallManagementDomainTestBase<TStartupModule> : BanquetHallManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
