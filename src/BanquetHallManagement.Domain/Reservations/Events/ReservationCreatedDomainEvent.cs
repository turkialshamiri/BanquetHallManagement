namespace BanquetHallManagement.Reservations.Events;

/// <summary>
/// Raised when a reservation is successfully created. Expand with handlers as the domain evolves.
/// </summary>
public class ReservationCreatedDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public ReservationCreatedDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
