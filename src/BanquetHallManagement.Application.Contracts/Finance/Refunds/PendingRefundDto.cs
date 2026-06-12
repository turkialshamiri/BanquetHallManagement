using System;

namespace BanquetHallManagement.Finance.Refunds;

public class PendingRefundDto
{
    public Guid ReservationId { get; set; }

    public string ReservationNumber { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string HallName { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal InstallmentsPaid { get; set; }

    public decimal RefundableAmount { get; set; }

    public decimal LiabilityAmount { get; set; }

    public decimal RefundAmount { get; set; }

    public Guid? LiabilityJournalEntryId { get; set; }

    public string? LiabilityJournalEntryNumber { get; set; }

    public string StatusCode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }
}
