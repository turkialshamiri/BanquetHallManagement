using System.Threading.Tasks;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Reservations.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class HallAccessCardMarkUsedHandler :
    ILocalEventHandler<HallEntryConfirmedDomainEvent>,
    ITransientDependency
{
    private readonly IHallAccessCardRepository _hallAccessCardRepository;

    public HallAccessCardMarkUsedHandler(IHallAccessCardRepository hallAccessCardRepository)
    {
        _hallAccessCardRepository = hallAccessCardRepository;
    }

    public async Task HandleEventAsync(HallEntryConfirmedDomainEvent eventData)
    {
        var card = await _hallAccessCardRepository.FindByReservationIdAsync(
            eventData.Snapshot.ReservationId);

        if (card == null || card.IsUsed)
        {
            return;
        }

        card.MarkAsUsed();
        await _hallAccessCardRepository.UpdateAsync(card);
    }
}
