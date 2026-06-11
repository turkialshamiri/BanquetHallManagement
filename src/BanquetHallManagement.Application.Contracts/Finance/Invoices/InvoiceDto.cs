using System;

namespace BanquetHallManagement.Finance.Invoices;

public class InvoiceDto
{
    public Guid Id { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public Guid ReservationId { get; set; }

    public Guid PaymentId { get; set; }

    public string InvoiceType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime IssuedAt { get; set; }
}
