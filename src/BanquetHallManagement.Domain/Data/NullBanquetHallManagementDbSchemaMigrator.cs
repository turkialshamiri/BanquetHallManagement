using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace BanquetHallManagement.Data;

/* This is used if database provider does't define
 * IBanquetHallManagementDbSchemaMigrator implementation.
 */
public class NullBanquetHallManagementDbSchemaMigrator : IBanquetHallManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
