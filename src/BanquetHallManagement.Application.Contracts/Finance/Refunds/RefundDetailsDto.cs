using System;
using System.Collections.Generic;
using BanquetHallManagement.Finance.Payments;

namespace BanquetHallManagement.Finance.Refunds;

public class RefundDetailsDto
{
    public Guid ReservationId { get; set; }

    public string ReservationNumber { get; set; } = null!;

    public DateTime ReservationDate { get; set; }

    public DateTime EventDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public string HallName { get; set; } = null!;

    public string ReservationStatus { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal InstallmentsPaid { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RefundableAmount { get; set; }

    public decimal RemainingBalance { get; set; }

    public decimal LiabilityAmount { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public string RefundStatusCode { get; set; } = null!;

    public string RefundStatus { get; set; } = null!;

    public bool IsRefundEligible { get; set; }

    public string? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Guid? LiabilityJournalEntryId { get; set; }

    public string? LiabilityJournalEntryNumber { get; set; }

    public Guid? RefundJournalEntryId { get; set; }

    public string? RefundJournalEntryNumber { get; set; }

    public List<PaymentDto> Payments { get; set; } = [];
}
