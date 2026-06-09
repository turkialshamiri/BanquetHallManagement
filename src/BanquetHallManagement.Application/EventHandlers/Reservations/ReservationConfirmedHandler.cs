using System.Threading.Tasks;
using BanquetHallManagement.Events.Reservations;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Reservations;

public class ReservationConfirmedHandler :
    IDistributedEventHandler<ReservationConfirmedEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationConfirmedHandler> _logger;

    public ReservationConfirmedHandler(ILogger<ReservationConfirmedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationConfirmedEto eventData)
    {
        ReservationLifecycleEventLogger.Log(_logger, "confirmed", eventData);

        return Task.CompletedTask;
    }
}
