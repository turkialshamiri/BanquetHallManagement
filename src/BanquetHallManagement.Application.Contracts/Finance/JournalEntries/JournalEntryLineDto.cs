using System;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalEntryLineDto
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string AccountCode { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public string? Description { get; set; }
}
