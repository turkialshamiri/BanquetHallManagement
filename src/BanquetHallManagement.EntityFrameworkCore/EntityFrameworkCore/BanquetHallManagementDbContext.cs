using BanquetHallManagement.Customers;
using BanquetHallManagement.Dashboard;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Sequences;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Services;
using BanquetHallManagement.EntityFrameworkCore.Catalog;
using BanquetHallManagement.EntityFrameworkCore.Catalog.Configurations;
using BanquetHallManagement.EntityFrameworkCore.Dashboard.Configurations;
using BanquetHallManagement.EntityFrameworkCore.Finance;
using BanquetHallManagement.EntityFrameworkCore.Finance.Configurations;
using BanquetHallManagement.EntityFrameworkCore.Reservations;
using BanquetHallManagement.EntityFrameworkCore.Reservations.Configurations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ReplaceDbContext(typeof(ICatalogDbContext))]
[ReplaceDbContext(typeof(IReservationsDbContext))]
[ReplaceDbContext(typeof(IFinanceDbContext))]
[ConnectionStringName("Default")]
public class BanquetHallManagementDbContext :
    AbpDbContext<BanquetHallManagementDbContext>,
    ICatalogDbContext,
    IReservationsDbContext,
    IFinanceDbContext,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Account> FinanceAccounts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<FinanceNumberSequence> FinanceNumberSequences { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<HallAccessCard> HallAccessCards { get; set; }
    public DbSet<DashboardMetricsSnapshot> DashboardMetricsSnapshots { get; set; }

    #region Entities from the modules

    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public BanquetHallManagementDbContext(DbContextOptions<BanquetHallManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.ConfigureCatalog();
        builder.ConfigureReservations();
        builder.ConfigureFinance();
        builder.ConfigureDashboard();
    }
}
