using Microsoft.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Catalog.Configurations;

public static class CatalogDbContextModelCreatingExtensions
{
    public static void ConfigureCatalog(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new HallConfiguration());
        builder.ApplyConfiguration(new CustomerConfiguration());
        builder.ApplyConfiguration(new ServiceConfiguration());
    }
}
