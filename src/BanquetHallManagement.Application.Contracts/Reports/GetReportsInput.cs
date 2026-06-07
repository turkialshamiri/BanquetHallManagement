using System;
using BanquetHallManagement.Enums;

namespace BanquetHallManagement.Reports;

public class GetReportsInput
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public Guid? HallId { get; set; }

    public ReservationStatus? Status { get; set; }
}
