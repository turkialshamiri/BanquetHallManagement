namespace BanquetHallManagement.Reports;

public class MonthlyRevenueDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthLabel { get; set; } = string.Empty;

    public long ReservationCount { get; set; }

    public decimal Revenue { get; set; }
}
