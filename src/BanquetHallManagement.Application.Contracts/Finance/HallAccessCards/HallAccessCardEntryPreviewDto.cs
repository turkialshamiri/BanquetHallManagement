using System;

namespace BanquetHallManagement.Finance.HallAccessCards;

public class HallAccessCardEntryPreviewDto
{
    public Guid ReservationId { get; set; }

    public string ReservationNumber { get; set; } = null!;

    public string ReservationStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public decimal PaidAmount { get; set; }

    public string CustomerName { get; set; } = null!;

    public string HallName { get; set; } = null!;

    public Guid HallAccessCardId { get; set; }

    public string CardNumber { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public TimeSpan EntryTime { get; set; }

    public TimeSpan ExitTime { get; set; }

    public string EmployeeName { get; set; } = null!;

    public bool CanConfirmEntry { get; set; }
}
