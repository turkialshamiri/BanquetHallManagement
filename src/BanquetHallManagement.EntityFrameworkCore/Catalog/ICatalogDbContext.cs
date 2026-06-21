using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Services;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Catalog;

[ConnectionStringName("Default")]
public interface ICatalogDbContext : IEfCoreDbContext
{
    DbSet<Hall> Halls { get; }

    DbSet<Service> Services { get; }

    DbSet<Customer> Customers { get; }
}
