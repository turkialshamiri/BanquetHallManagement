using System.Collections.Generic;

namespace BanquetHallManagement.Reports;

public class ReportsResultDto
{
    public ReportSummaryDto Summary { get; set; } = new();

    public RevenueAnalyticsDto RevenueAnalytics { get; set; } = new();

    public ReservationStatisticsDto ReservationStatistics { get; set; } = new();

    public List<HallPerformanceDto> HallPerformance { get; set; } = [];

    public List<CustomerActivityDto> CustomerActivity { get; set; } = [];

    public List<MonthlyRevenueDto> MonthlyReport { get; set; } = [];
}
