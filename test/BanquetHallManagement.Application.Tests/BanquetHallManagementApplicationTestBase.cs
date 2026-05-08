using Volo.Abp.Modularity;

namespace BanquetHallManagement;

public abstract class BanquetHallManagementApplicationTestBase<TStartupModule> : BanquetHallManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
