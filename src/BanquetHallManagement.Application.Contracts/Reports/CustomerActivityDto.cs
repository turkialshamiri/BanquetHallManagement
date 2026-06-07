namespace BanquetHallManagement.Reports;

public class CustomerActivityDto
{
    public string CustomerName { get; set; } = string.Empty;

    public long ReservationCount { get; set; }

    public decimal TotalSpent { get; set; }
}
