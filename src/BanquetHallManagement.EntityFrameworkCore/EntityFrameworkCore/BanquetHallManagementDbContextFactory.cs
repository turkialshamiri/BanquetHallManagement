using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BanquetHallManagement.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class BanquetHallManagementDbContextFactory : IDesignTimeDbContextFactory<BanquetHallManagementDbContext>
{
    public BanquetHallManagementDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        BanquetHallManagementEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<BanquetHallManagementDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new BanquetHallManagementDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BanquetHallManagement.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
