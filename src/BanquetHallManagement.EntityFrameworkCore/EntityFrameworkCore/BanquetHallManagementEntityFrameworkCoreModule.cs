using BanquetHallManagement.EntityFrameworkCore.Repositories;
using BanquetHallManagement.EntityFrameworkCore.Finance.Refunds;
using BanquetHallManagement.EntityFrameworkCore.Reports;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Reports;
using BanquetHallManagement.Reservations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Studio;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.Uow;

namespace BanquetHallManagement.EntityFrameworkCore;

[DependsOn(
    typeof(BanquetHallManagementDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
    )]
public class BanquetHallManagementEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {

        BanquetHallManagementEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<BanquetHallManagementDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Reservation, EfCoreReservationRepository>();
            options.AddRepository<JournalEntry, EfCoreJournalEntryRepository>();
            options.AddRepository<Invoice, EfCoreInvoiceRepository>();
            options.AddRepository<HallAccessCard, EfCoreHallAccessCardRepository>();
        });

        context.Services.AddTransient<IReportQueryExecutor, EfCoreReportQueryExecutor>();
        context.Services.AddTransient<IRefundQueryRepository, EfCoreRefundQueryRepository>();

        if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        {
            return;
        }

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also BanquetHallManagementDbContextFactory for EF Core tooling. */

            options.UseSqlServer();

        });

        Configure<AbpUnitOfWorkDefaultOptions>(options =>
        {
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Auto;
        });
    }
}
