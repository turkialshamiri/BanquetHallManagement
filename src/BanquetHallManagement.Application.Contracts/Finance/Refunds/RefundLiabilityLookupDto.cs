using System;

namespace BanquetHallManagement.Finance.Refunds;

public class RefundLiabilityLookupDto
{
    public Guid ReservationId { get; set; }

    public string ReservationNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string HallName { get; set; } = null!;

    public decimal PaidAmount { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal InstallmentsPaid { get; set; }

    public decimal RefundableAmount { get; set; }

    public decimal LiabilityAmount { get; set; }

    public Guid? LiabilityJournalEntryId { get; set; }

    public string? LiabilityJournalEntryNumber { get; set; }

    public string StatusCode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CancelledAt { get; set; }
}
