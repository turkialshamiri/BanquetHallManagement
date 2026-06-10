using System;
using BanquetHallManagement.Enums;

namespace BanquetHallManagement.Finance.Payments.Events;

public class PaymentReceivedDomainEvent
{
    public Guid PaymentId { get; }

    public Guid ReservationId { get; }

    public decimal Amount { get; }

    public PaymentType PaymentType { get; }

    public DateTime PaymentDate { get; }

    public string ReceiptNumber { get; }

    public PaymentReceivedDomainEvent(
        Guid paymentId,
        Guid reservationId,
        decimal amount,
        PaymentType paymentType,
        DateTime paymentDate,
        string receiptNumber)
    {
        PaymentId = paymentId;
        ReservationId = reservationId;
        Amount = amount;
        PaymentType = paymentType;
        PaymentDate = paymentDate;
        ReceiptNumber = receiptNumber;
    }
}
