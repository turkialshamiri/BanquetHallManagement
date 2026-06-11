using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Reservations;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.HallAccessCards;

public class HallAccessCardManager : DomainService
{
    private readonly IHallAccessCardRepository _hallAccessCardRepository;
    private readonly ICardNumberGenerator _cardNumberGenerator;

    public HallAccessCardManager(
        IHallAccessCardRepository hallAccessCardRepository,
        ICardNumberGenerator cardNumberGenerator)
    {
        _hallAccessCardRepository = hallAccessCardRepository;
        _cardNumberGenerator = cardNumberGenerator;
    }

    public async Task<HallAccessCard> CreateForReservationAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var existingCard = await _hallAccessCardRepository.FindByReservationIdAsync(
            reservation.Id,
            cancellationToken);

        if (existingCard != null)
        {
            return existingCard;
        }

        var cardNumber = await _cardNumberGenerator.GenerateAsync(cancellationToken);

        var card = new HallAccessCard(
            GuidGenerator.Create(),
            reservation.Id,
            cardNumber,
            Clock.Now,
            reservation.EventDate,
            reservation.StartTime,
            reservation.EndTime);

        await _hallAccessCardRepository.InsertAsync(card, autoSave: false, cancellationToken);

        return card;
    }
}
