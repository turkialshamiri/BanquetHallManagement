namespace BanquetHallManagement.Reports;

public class HallPerformanceDto
{
    public string HallName { get; set; } = string.Empty;

    public long ReservationCount { get; set; }

    public decimal Revenue { get; set; }

    public decimal AverageGuests { get; set; }

    public decimal OccupancyRate { get; set; }
}
