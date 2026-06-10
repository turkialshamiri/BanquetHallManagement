using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalEntryLine : Entity<Guid>
{
    public Guid JournalEntryId { get; private set; }

    public Guid AccountId { get; private set; }

    public decimal Debit { get; private set; }

    public decimal Credit { get; private set; }

    public string? Description { get; private set; }

    protected JournalEntryLine()
    {
    }

    internal JournalEntryLine(
        Guid id,
        Guid journalEntryId,
        Guid accountId,
        decimal debit,
        decimal credit,
        string? description)
    {
        ValidateAmounts(debit, credit);

        Id = id;
        JournalEntryId = journalEntryId;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
        Description = description;
    }

    internal static void ValidateAmounts(decimal debit, decimal credit)
    {
        if (debit < 0 || credit < 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.JournalEntryLineAmountInvalid);
        }

        if (debit > 0 && credit > 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.JournalEntryLineAmountInvalid);
        }

        if (debit == 0 && credit == 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.JournalEntryLineAmountInvalid);
        }
    }
}
