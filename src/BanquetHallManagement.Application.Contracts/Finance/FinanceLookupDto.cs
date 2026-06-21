using System;

namespace BanquetHallManagement.Finance;

/// <summary>
/// Read-only finance summary for cross-module lookups.
/// </summary>
public class FinanceLookupDto
{
    public Guid Id { get; set; }

    public Guid ReservationId { get; set; }
}
