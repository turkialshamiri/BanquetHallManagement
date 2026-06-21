using System;
using BanquetHallManagement.Enums;

namespace BanquetHallManagement.Reservations;

/// <summary>
/// Read-only reservation summary for cross-module lookups.
/// </summary>
public class ReservationLookupDto
{
    public Guid Id { get; set; }

    public string? ReservationNumber { get; set; }

    public ReservationStatus Status { get; set; }
}
