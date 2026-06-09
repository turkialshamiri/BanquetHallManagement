using System;
using BanquetHallManagement.Events;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.Events.Notifications;

[EventName(BanquetHallManagementEventNames.ReservationNotification)]
public class ReservationNotificationEto
{
    public Guid ReservationId { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string HallName { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string NotificationType { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
