using System.Threading.Tasks;
using BanquetHallManagement.Events.Reservations;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Reservations;

public class ReservationCompletedHandler :
    IDistributedEventHandler<ReservationCompletedEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationCompletedHandler> _logger;

    public ReservationCompletedHandler(ILogger<ReservationCompletedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationCompletedEto eventData)
    {
        ReservationLifecycleEventLogger.Log(_logger, "completed", eventData);

        return Task.CompletedTask;
    }
}
