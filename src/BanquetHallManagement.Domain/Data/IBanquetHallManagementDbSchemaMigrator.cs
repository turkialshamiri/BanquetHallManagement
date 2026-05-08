using System.Threading.Tasks;

namespace BanquetHallManagement.Data;

public interface IBanquetHallManagementDbSchemaMigrator
{
    Task MigrateAsync();
}
