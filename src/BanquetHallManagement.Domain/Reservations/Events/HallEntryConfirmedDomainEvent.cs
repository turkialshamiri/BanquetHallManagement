namespace BanquetHallManagement.Reservations.Events;

public class HallEntryConfirmedDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public HallEntryConfirmedDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
