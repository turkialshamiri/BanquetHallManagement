using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Invoices.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Finance.Invoices;

public class Invoice : FullAuditedAggregateRoot<Guid>
{
    public string InvoiceNumber { get; private set; } = null!;

    public Guid ReservationId { get; private set; }

    public Guid PaymentId { get; private set; }

    public InvoiceType InvoiceType { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime IssuedAt { get; private set; }

    protected Invoice()
    {
    }

    public Invoice(
        Guid id,
        string invoiceNumber,
        Guid reservationId,
        Guid paymentId,
        InvoiceType invoiceType,
        decimal amount,
        DateTime issuedAt)
    {
        if (amount <= 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentAmountInvalid);
        }

        Id = id;
        InvoiceNumber = Check.NotNullOrWhiteSpace(invoiceNumber, nameof(invoiceNumber), maxLength: 50);
        ReservationId = reservationId;
        PaymentId = paymentId;
        InvoiceType = invoiceType;
        Amount = amount;
        IssuedAt = issuedAt;

        if (invoiceType == InvoiceType.Deposit)
        {
            AddLocalEvent(new DepositInvoiceCreatedEvent(
                id,
                invoiceNumber,
                reservationId,
                paymentId,
                amount,
                invoiceType,
                issuedAt));
        }
    }
}
