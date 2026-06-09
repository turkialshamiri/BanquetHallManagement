using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Events;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.Events.Audit;

[EventName(BanquetHallManagementEventNames.ReservationAudit)]
public class ReservationAuditEto
{
    public Guid ReservationId { get; set; }

    public string Action { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public Guid? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public Guid HallId { get; set; }

    public string HallName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public ReservationStatus ReservationStatus { get; set; }
}
