using System;

namespace BanquetHallManagement.Finance.Refunds;

public class PendingRefundDto
{
    public Guid ReservationId { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RefundAmount { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }
}
