using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Reservations;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Invoices;

public class InvoiceManager : DomainService, IInvoiceManager
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IRepository<Payment, Guid> _paymentRepository;

    public InvoiceManager(
        IInvoiceRepository invoiceRepository,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IRepository<Payment, Guid> paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _paymentRepository = paymentRepository;
    }

    public async Task<Invoice> CreateDepositInvoiceAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        var existingInvoice = await FindExistingInvoiceAsync(payment, cancellationToken);
        if (existingInvoice != null)
        {
            payment.LinkInvoice(existingInvoice.Id);
            return existingInvoice;
        }

        if (payment.PaymentType != PaymentType.Deposit)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.InvoiceCreationNotSupported)
                .WithData("PaymentType", payment.PaymentType.ToString());
        }

        var invoiceNumber = await _invoiceNumberGenerator.GenerateAsync(cancellationToken);

        var invoice = new Invoice(
            GuidGenerator.Create(),
            invoiceNumber,
            payment.ReservationId,
            payment.Id,
            InvoiceType.Deposit,
            payment.Amount,
            payment.PaymentDate);

        await _invoiceRepository.InsertAsync(invoice, autoSave: false, cancellationToken);

        payment.LinkInvoice(invoice.Id);

        return invoice;
    }

    private async Task<Invoice?> FindExistingInvoiceAsync(
        Payment payment,
        CancellationToken cancellationToken)
    {
        if (payment.InvoiceId.HasValue)
        {
            var linkedInvoice = await _invoiceRepository.FindAsync(
                payment.InvoiceId.Value,
                cancellationToken: cancellationToken);

            if (linkedInvoice != null)
            {
                return linkedInvoice;
            }
        }

        return await _invoiceRepository.FindByPaymentIdAsync(payment.Id, cancellationToken);
    }

    public async Task<Invoice> EnsureFullyPaidInvoiceAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var existingFinal = await _invoiceRepository.FindByReservationIdAndTypeAsync(
            reservation.Id,
            InvoiceType.Final,
            cancellationToken);

        if (existingFinal != null)
        {
            return existingFinal;
        }

        var existingSettlement = await _invoiceRepository.FindSettlementInvoiceByReservationAsync(
            reservation.Id,
            reservation.TotalPrice,
            cancellationToken);

        if (existingSettlement != null)
        {
            return existingSettlement;
        }

        var payment = await FindLatestPaymentAsync(reservation.Id, cancellationToken);
        if (payment == null)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.PaymentNotFound)
                .WithData("ReservationId", reservation.Id);
        }

        var invoiceNumber = await _invoiceNumberGenerator.GenerateAsync(cancellationToken);

        var invoice = new Invoice(
            GuidGenerator.Create(),
            invoiceNumber,
            reservation.Id,
            payment.Id,
            InvoiceType.Final,
            reservation.TotalPrice,
            Clock.Now);

        await _invoiceRepository.InsertAsync(invoice, autoSave: false, cancellationToken);

        payment.LinkInvoice(invoice.Id);

        return invoice;
    }

    private async Task<Payment?> FindLatestPaymentAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetListAsync(
            payment => payment.ReservationId == reservationId,
            cancellationToken: cancellationToken);

        return payments
            .OrderByDescending(payment => payment.PaymentDate)
            .ThenByDescending(payment => payment.CreationTime)
            .FirstOrDefault();
    }
}
