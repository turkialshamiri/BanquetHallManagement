using System;

namespace BanquetHallManagement.Finance.HallAccessCards;

public class HallAccessCardDto
{
    public Guid Id { get; set; }

    public Guid ReservationId { get; set; }

    public string CardNumber { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime EventDate { get; set; }

    public TimeSpan EntryTime { get; set; }

    public TimeSpan ExitTime { get; set; }

    public bool IsUsed { get; set; }
}
