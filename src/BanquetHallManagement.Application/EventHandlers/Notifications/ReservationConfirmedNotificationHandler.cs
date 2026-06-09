using System.Threading.Tasks;
using BanquetHallManagement.Events.Notifications;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Notifications;

public class ReservationConfirmedNotificationHandler :
    IDistributedEventHandler<ReservationNotificationEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationConfirmedNotificationHandler> _logger;

    public ReservationConfirmedNotificationHandler(
        ILogger<ReservationConfirmedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationNotificationEto eventData)
    {
        if (eventData.NotificationType != ReservationNotificationTypes.Confirmed)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Notification queued: Type={NotificationType}, Customer={CustomerName}, " +
            "Reservation={ReservationId}, Hall={HallName}, EventDate={EventDate}, Message={Message}",
            eventData.NotificationType,
            eventData.CustomerName,
            eventData.ReservationId,
            eventData.HallName,
            eventData.EventDate,
            eventData.Message);

        return Task.CompletedTask;
    }
}
