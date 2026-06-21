using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Invoices;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Repositories;

public class EfCoreInvoiceRepository :
    EfCoreRepository<BanquetHallManagementDbContext, Invoice, Guid>,
    IInvoiceRepository
{
    public EfCoreInvoiceRepository(IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Invoice?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var trackedInvoice = dbContext.ChangeTracker
            .Entries<Invoice>()
            .Select(entry => entry.Entity)
            .FirstOrDefault(invoice => invoice.PaymentId == paymentId);

        if (trackedInvoice != null)
        {
            return trackedInvoice;
        }

        var query = await GetQueryableAsync();

        return await query.FirstOrDefaultAsync(
            invoice => invoice.PaymentId == paymentId,
            cancellationToken);
    }

    public async Task<Invoice?> FindByReservationIdAndTypeAsync(
        Guid reservationId,
        InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await query.FirstOrDefaultAsync(
            invoice => invoice.ReservationId == reservationId &&
                       invoice.InvoiceType == invoiceType,
            cancellationToken);
    }

    public async Task<Invoice?> FindSettlementInvoiceByReservationAsync(
        Guid reservationId,
        decimal totalPrice,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await query
            .Where(invoice => invoice.ReservationId == reservationId && invoice.Amount >= totalPrice)
            .OrderByDescending(invoice => invoice.IssuedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
