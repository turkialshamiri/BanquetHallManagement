using System.Threading.Tasks;
using BanquetHallManagement.Events.Audit;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Audit;

public class ReservationDeletedAuditHandler :
    IDistributedEventHandler<ReservationAuditEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationDeletedAuditHandler> _logger;

    public ReservationDeletedAuditHandler(ILogger<ReservationDeletedAuditHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationAuditEto eventData)
    {
        if (eventData.Action != ReservationAuditActions.Deleted)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Reservation Audit: Action={Action}, ReservationId={ReservationId}, User={UserName}, " +
            "UserId={UserId}, Timestamp={Timestamp}, Hall={HallName}, Customer={CustomerName}, Status={ReservationStatus}",
            eventData.Action,
            eventData.ReservationId,
            eventData.UserName,
            eventData.UserId,
            eventData.Timestamp,
            eventData.HallName,
            eventData.CustomerName,
            eventData.ReservationStatus);

        return Task.CompletedTask;
    }
}
