using System.Threading.Tasks;
using BanquetHallManagement.Events.Reservations;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Reservations;

public class ReservationCancelledHandler :
    IDistributedEventHandler<ReservationCancelledEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationCancelledHandler> _logger;

    public ReservationCancelledHandler(ILogger<ReservationCancelledHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationCancelledEto eventData)
    {
        ReservationLifecycleEventLogger.Log(_logger, "cancelled", eventData);

        return Task.CompletedTask;
    }
}
