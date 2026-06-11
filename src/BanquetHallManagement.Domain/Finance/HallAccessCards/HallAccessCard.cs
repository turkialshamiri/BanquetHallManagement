using System;
using BanquetHallManagement.Finance.HallAccessCards.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Finance.HallAccessCards;

public class HallAccessCard : FullAuditedAggregateRoot<Guid>
{
    public Guid ReservationId { get; private set; }

    public string CardNumber { get; private set; } = null!;

    public DateTime IssuedAt { get; private set; }

    public DateTime EventDate { get; private set; }

    public TimeSpan EntryTime { get; private set; }

    public TimeSpan ExitTime { get; private set; }

    public bool IsUsed { get; private set; }

    protected HallAccessCard()
    {
    }

    public HallAccessCard(
        Guid id,
        Guid reservationId,
        string cardNumber,
        DateTime issuedAt,
        DateTime eventDate,
        TimeSpan entryTime,
        TimeSpan exitTime)
    {
        Id = id;
        ReservationId = reservationId;
        CardNumber = Check.NotNullOrWhiteSpace(cardNumber, nameof(cardNumber), maxLength: 50);
        IssuedAt = issuedAt;
        EventDate = eventDate;
        EntryTime = entryTime;
        ExitTime = exitTime;

        AddLocalEvent(new HallAccessCardCreatedDomainEvent(
            id,
            reservationId,
            cardNumber,
            eventDate,
            entryTime,
            exitTime));
    }
}
