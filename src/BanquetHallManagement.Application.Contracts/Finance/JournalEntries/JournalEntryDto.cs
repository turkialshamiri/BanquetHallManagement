using System;
using System.Collections.Generic;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalEntryDto
{
    public Guid Id { get; set; }

    public string EntryNumber { get; set; } = null!;

    public DateTime EntryDate { get; set; }

    public string SourceType { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? ReservationId { get; set; }

    public Guid? PaymentId { get; set; }

    public bool IsPosted { get; set; }

    public DateTime? PostedTime { get; set; }

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public List<JournalEntryLineDto> Lines { get; set; } = [];
}
