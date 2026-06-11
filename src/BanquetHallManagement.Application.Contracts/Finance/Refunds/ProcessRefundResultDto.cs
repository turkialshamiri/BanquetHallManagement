using System;

namespace BanquetHallManagement.Finance.Refunds;

public class ProcessRefundResultDto
{
    public Guid ReservationId { get; set; }

    public Guid JournalEntryId { get; set; }

    public string EntryNumber { get; set; } = null!;

    public decimal RefundAmount { get; set; }
}
