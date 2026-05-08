using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BanquetHallManagement.Data;
using Volo.Abp.DependencyInjection;

namespace BanquetHallManagement.EntityFrameworkCore;

public class EntityFrameworkCoreBanquetHallManagementDbSchemaMigrator
    : IBanquetHallManagementDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreBanquetHallManagementDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the BanquetHallManagementDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<BanquetHallManagementDbContext>()
            .Database
            .MigrateAsync();
    }
}
