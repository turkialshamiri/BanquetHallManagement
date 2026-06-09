using System;
using BanquetHallManagement.Enums;

namespace BanquetHallManagement.Events.Reservations;

/// <summary>
/// Shared payload for reservation lifecycle distributed events.
/// Handlers can consume derived types without re-querying the database.
/// </summary>
public abstract class ReservationEventEto
{
    public Guid ReservationId { get; set; }

    public Guid HallId { get; set; }

    public string HallName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public int GuestsCount { get; set; }

    public decimal TotalPrice { get; set; }

    public ReservationStatus ReservationStatus { get; set; }

    public Guid? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}
