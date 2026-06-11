namespace BanquetHallManagement.Reservations.Events;

public class ReservationFullyPaidDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public ReservationFullyPaidDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
