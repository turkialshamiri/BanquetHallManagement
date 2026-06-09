using System.Threading.Tasks;
using BanquetHallManagement.Events.Reservations;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace BanquetHallManagement.EventHandlers.Reservations;

public class ReservationDeletedHandler :
    IDistributedEventHandler<ReservationDeletedEto>,
    ITransientDependency
{
    private readonly ILogger<ReservationDeletedHandler> _logger;

    public ReservationDeletedHandler(ILogger<ReservationDeletedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationDeletedEto eventData)
    {
        ReservationLifecycleEventLogger.Log(_logger, "deleted", eventData);

        return Task.CompletedTask;
    }
}
