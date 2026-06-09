namespace BanquetHallManagement.Reservations.Events;

/// <summary>
/// Raised when a reservation is cancelled. Expand with handlers as the domain evolves.
/// </summary>
public class ReservationCancelledDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public ReservationCancelledDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
