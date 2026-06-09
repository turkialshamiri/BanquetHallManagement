namespace BanquetHallManagement.Reservations.Events;

/// <summary>
/// Raised when a reservation is marked for deletion. Expand with handlers as the domain evolves.
/// </summary>
public class ReservationDeletedDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public ReservationDeletedDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
