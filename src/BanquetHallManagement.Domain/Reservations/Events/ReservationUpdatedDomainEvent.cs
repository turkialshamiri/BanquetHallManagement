namespace BanquetHallManagement.Reservations.Events;

/// <summary>
/// Raised when a reservation is successfully updated. Expand with handlers as the domain evolves.
/// </summary>
public class ReservationUpdatedDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public ReservationUpdatedDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
