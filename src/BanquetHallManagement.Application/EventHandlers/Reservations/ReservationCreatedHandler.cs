using System.Threading.Tasks;
using BanquetHallManagement.Events.Reservations;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Reservations;

public class ReservationCreatedHandler :
    IDistributedEventHandler<ReservationCreatedEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationCreatedHandler> _logger;

    public ReservationCreatedHandler(ILogger<ReservationCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationCreatedEto eventData)
    {
        ReservationLifecycleEventLogger.Log(_logger, "created", eventData);

        return Task.CompletedTask;
    }
}
