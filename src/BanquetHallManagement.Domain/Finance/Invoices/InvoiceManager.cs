using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Invoices;

public class InvoiceManager : DomainService, IInvoiceManager
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;

    public InvoiceManager(
        IInvoiceRepository invoiceRepository,
        IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
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
}
