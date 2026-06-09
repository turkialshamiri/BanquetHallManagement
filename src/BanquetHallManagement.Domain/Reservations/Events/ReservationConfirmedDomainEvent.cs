namespace BanquetHallManagement.Reservations.Events;

/// <summary>
/// Raised when a reservation is confirmed. Expand with handlers as the domain evolves.
/// </summary>
public class ReservationConfirmedDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public ReservationConfirmedDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
