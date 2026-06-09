namespace BanquetHallManagement.Reservations.Events;

public interface IReservationDomainEvent
{
    ReservationEventSnapshot Snapshot { get; }
}
