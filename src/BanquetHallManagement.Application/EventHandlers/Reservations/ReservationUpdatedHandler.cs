using System.Threading.Tasks;
using BanquetHallManagement.Events.Reservations;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Reservations;

public class ReservationUpdatedHandler :
    IDistributedEventHandler<ReservationUpdatedEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationUpdatedHandler> _logger;

    public ReservationUpdatedHandler(ILogger<ReservationUpdatedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationUpdatedEto eventData)
    {
        ReservationLifecycleEventLogger.Log(_logger, "updated", eventData);

        return Task.CompletedTask;
    }
}
