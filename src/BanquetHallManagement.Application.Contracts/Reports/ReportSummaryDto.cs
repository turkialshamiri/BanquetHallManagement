namespace BanquetHallManagement.Reports;

public class ReportSummaryDto
{
    public long TotalReservations { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal AverageReservationValue { get; set; }

    public long ActiveCustomers { get; set; }
}
