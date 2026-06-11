using System;

namespace BanquetHallManagement.Finance.Invoices;

public class InvoicePrintDataDto
{
    public Guid InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public string InvoiceType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime IssuedAt { get; set; }

    public string ReceiptNumber { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public Guid ReservationId { get; set; }

    public DateTime EventDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int GuestsCount { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal PaidAmount { get; set; }

    public string ReservationStatus { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerCompany { get; set; }

    public string HallName { get; set; } = null!;

    public string HallLocation { get; set; } = null!;
}
