using System.Threading.Tasks;
using BanquetHallManagement.Finance.HallAccessCards.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class HallAccessCardCreatedHandler :
    ILocalEventHandler<HallAccessCardCreatedDomainEvent>,
    ITransientDependency
{
    private readonly ILogger<HallAccessCardCreatedHandler> _logger;

    public HallAccessCardCreatedHandler(ILogger<HallAccessCardCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(HallAccessCardCreatedDomainEvent eventData)
    {
        _logger.LogInformation(
            "Hall access card created | Card: {CardNumber} ({CardId}) | Reservation: {ReservationId} | EventDate: {EventDate} | Entry: {EntryTime} | Exit: {ExitTime}",
            eventData.CardNumber,
            eventData.HallAccessCardId,
            eventData.ReservationId,
            eventData.EventDate,
            eventData.EntryTime,
            eventData.ExitTime);

        return Task.CompletedTask;
    }
}
