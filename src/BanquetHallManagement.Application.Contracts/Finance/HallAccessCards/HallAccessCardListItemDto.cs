using System;

namespace BanquetHallManagement.Finance.HallAccessCards;

public class HallAccessCardListItemDto
{
    public Guid Id { get; set; }

    public Guid ReservationId { get; set; }

    public string ReservationNumber { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime EventDate { get; set; }

    public TimeSpan EntryTime { get; set; }

    public TimeSpan ExitTime { get; set; }

    public string CustomerName { get; set; } = null!;

    public string HallName { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public string ReservationStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public bool IsUsed { get; set; }
}
