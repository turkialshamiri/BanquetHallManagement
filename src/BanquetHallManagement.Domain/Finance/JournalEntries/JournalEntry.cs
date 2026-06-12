using System;
using System.Collections.Generic;
using System.Linq;
using BanquetHallManagement.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalEntry : FullAuditedAggregateRoot<Guid>
{
    public string EntryNumber { get; private set; } = null!;

    public DateTime EntryDate { get; private set; }

    public JournalEntrySourceType SourceType { get; private set; }

    public string? Description { get; private set; }

    public Guid? ReservationId { get; private set; }

    public Guid? PaymentId { get; private set; }

    public string? ReservationNumber { get; private set; }

    public string? CustomerName { get; private set; }

    public string? HallName { get; private set; }

    public string? EmployeeName { get; private set; }

    public bool IsPosted { get; private set; }

    public DateTime? PostedTime { get; private set; }

    public ICollection<JournalEntryLine> Lines { get; private set; } = new List<JournalEntryLine>();

    protected JournalEntry()
    {
    }

    public JournalEntry(
        Guid id,
        string entryNumber,
        DateTime entryDate,
        JournalEntrySourceType sourceType,
        string? description = null,
        Guid? reservationId = null,
        Guid? paymentId = null,
        JournalEntryBusinessMetadata? metadata = null)
    {
        Id = id;
        EntryNumber = Check.NotNullOrWhiteSpace(entryNumber, nameof(entryNumber), maxLength: 50);
        EntryDate = entryDate;
        SourceType = sourceType;
        Description = description;
        ReservationId = reservationId;
        PaymentId = paymentId;
        ApplyMetadata(metadata);
    }

    private void ApplyMetadata(JournalEntryBusinessMetadata? metadata)
    {
        if (metadata == null)
        {
            return;
        }

        ReservationNumber = metadata.ReservationNumber;
        CustomerName = metadata.CustomerName;
        HallName = metadata.HallName;
        EmployeeName = metadata.EmployeeName;
    }

    public void AddLine(
        Guid lineId,
        Guid accountId,
        decimal debit,
        decimal credit,
        string? description = null)
    {
        EnsureDraft();

        Lines.Add(new JournalEntryLine(
            lineId,
            Id,
            accountId,
            debit,
            credit,
            description));
    }

    public void Post(DateTime postedTime)
    {
        EnsureDraft();

        if (Lines.Count == 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.JournalEntryHasNoLines);
        }

        if (!IsBalanced())
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.JournalEntryUnbalanced);
        }

        IsPosted = true;
        PostedTime = postedTime;
    }

    public bool IsBalanced()
    {
        if (Lines.Count == 0)
        {
            return false;
        }

        return Lines.Sum(line => line.Debit) == Lines.Sum(line => line.Credit);
    }

    public decimal GetTotalDebit()
    {
        return Lines.Sum(line => line.Debit);
    }

    public decimal GetTotalCredit()
    {
        return Lines.Sum(line => line.Credit);
    }

    private void EnsureDraft()
    {
        if (IsPosted)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.JournalEntryAlreadyPosted);
        }
    }
}
