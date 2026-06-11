using System;

namespace BanquetHallManagement.Finance.HallAccessCards.Events;

public class HallAccessCardCreatedDomainEvent
{
    public Guid HallAccessCardId { get; }

    public Guid ReservationId { get; }

    public string CardNumber { get; }

    public DateTime EventDate { get; }

    public TimeSpan EntryTime { get; }

    public TimeSpan ExitTime { get; }

    public HallAccessCardCreatedDomainEvent(
        Guid hallAccessCardId,
        Guid reservationId,
        string cardNumber,
        DateTime eventDate,
        TimeSpan entryTime,
        TimeSpan exitTime)
    {
        HallAccessCardId = hallAccessCardId;
        ReservationId = reservationId;
        CardNumber = cardNumber;
        EventDate = eventDate;
        EntryTime = entryTime;
        ExitTime = exitTime;
    }
}
