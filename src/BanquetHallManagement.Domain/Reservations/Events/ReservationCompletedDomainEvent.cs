namespace BanquetHallManagement.Reservations.Events;

/// <summary>
/// Raised when a reservation is completed. Expand with handlers as the domain evolves.
/// </summary>
public class ReservationCompletedDomainEvent : IReservationDomainEvent
{
    public ReservationEventSnapshot Snapshot { get; }

    public ReservationCompletedDomainEvent(ReservationEventSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
