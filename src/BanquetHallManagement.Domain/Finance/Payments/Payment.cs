using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Payments.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Finance.Payments;

public class Payment : FullAuditedAggregateRoot<Guid>
{
    public Guid ReservationId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime PaymentDate { get; private set; }

    public PaymentType PaymentType { get; private set; }

    public string ReceiptNumber { get; private set; } = null!;

    public Guid? JournalEntryId { get; private set; }

    protected Payment()
    {
    }

    public Payment(
        Guid id,
        Guid reservationId,
        decimal amount,
        DateTime paymentDate,
        PaymentType paymentType,
        string receiptNumber)
    {
        if (amount <= 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentAmountInvalid);
        }

        Id = id;
        ReservationId = reservationId;
        Amount = amount;
        PaymentDate = paymentDate;
        PaymentType = paymentType;
        ReceiptNumber = Check.NotNullOrWhiteSpace(receiptNumber, nameof(receiptNumber), maxLength: 50);

        AddLocalEvent(new PaymentReceivedDomainEvent(
            id,
            reservationId,
            amount,
            paymentType,
            paymentDate,
            receiptNumber));
    }

    public void LinkJournalEntry(Guid journalEntryId)
    {
        if (JournalEntryId.HasValue)
        {
            if (JournalEntryId.Value == journalEntryId)
            {
                return;
            }

            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.JournalEntryDuplicatePosting);
        }

        JournalEntryId = journalEntryId;
    }
}
