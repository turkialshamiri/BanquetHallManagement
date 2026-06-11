using System;

namespace BanquetHallManagement.Finance.HallAccessCards;

public class HallAccessCardPrintDataDto
{
    public Guid HallAccessCardId { get; set; }

    public string CardNumber { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public Guid ReservationId { get; set; }

    public DateTime EventDate { get; set; }

    public TimeSpan EntryTime { get; set; }

    public TimeSpan ExitTime { get; set; }

    public int GuestsCount { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerCompany { get; set; }

    public string HallName { get; set; } = null!;

    public string HallLocation { get; set; } = null!;
}
