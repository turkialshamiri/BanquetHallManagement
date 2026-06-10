using System;

namespace BanquetHallManagement.Finance.Payments;

public class PaymentDto
{
    public Guid Id { get; set; }

    public Guid ReservationId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentType { get; set; } = null!;

    public string ReceiptNumber { get; set; } = null!;
}
