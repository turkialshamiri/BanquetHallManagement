namespace BanquetHallManagement.Reports;

public class ReservationStatisticsDto
{
    public long PendingCount { get; set; }

    public long ConfirmedCount { get; set; }

    public long CancelledCount { get; set; }

    public long CompletedCount { get; set; }
}
