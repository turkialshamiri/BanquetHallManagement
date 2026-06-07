namespace BanquetHallManagement.Reports;

public class RevenueAnalyticsDto
{
    public RevenuePeriodDto Today { get; set; } = new();

    public RevenuePeriodDto ThisMonth { get; set; } = new();

    public RevenuePeriodDto ThisYear { get; set; } = new();
}
