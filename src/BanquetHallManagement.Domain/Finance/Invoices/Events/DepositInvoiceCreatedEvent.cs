using System;
using BanquetHallManagement.Enums;

namespace BanquetHallManagement.Finance.Invoices.Events;

public class DepositInvoiceCreatedEvent
{
    public Guid InvoiceId { get; }

    public string InvoiceNumber { get; }

    public Guid ReservationId { get; }

    public Guid PaymentId { get; }

    public decimal Amount { get; }

    public InvoiceType InvoiceType { get; }

    public DateTime IssuedAt { get; }

    public DepositInvoiceCreatedEvent(
        Guid invoiceId,
        string invoiceNumber,
        Guid reservationId,
        Guid paymentId,
        decimal amount,
        InvoiceType invoiceType,
        DateTime issuedAt)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        ReservationId = reservationId;
        PaymentId = paymentId;
        Amount = amount;
        InvoiceType = invoiceType;
        IssuedAt = issuedAt;
    }
}
