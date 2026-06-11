using System;
using BanquetHallManagement.Enums;

namespace BanquetHallManagement.Reservations.Events;

/// <summary>
/// Immutable snapshot of reservation state at the time a domain event is raised.
/// Used by integration handlers without re-querying the aggregate.
/// </summary>
public class ReservationEventSnapshot
{
    public Guid ReservationId { get; init; }

    public Guid HallId { get; init; }

    public Guid CustomerId { get; init; }

    public DateTime EventDate { get; init; }

    public int GuestsCount { get; init; }

    public decimal TotalPrice { get; init; }

    public decimal PaidAmount { get; init; }

    public ReservationStatus ReservationStatus { get; init; }

    public static ReservationEventSnapshot FromReservation(Reservation reservation)
    {
        return new ReservationEventSnapshot
        {
            ReservationId = reservation.Id,
            HallId = reservation.HallId,
            CustomerId = reservation.CustomerId,
            EventDate = reservation.EventDate,
            GuestsCount = reservation.GuestsCount,
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            ReservationStatus = reservation.Status,
        };
    }
}
