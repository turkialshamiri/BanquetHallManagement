using System;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class RevenueRecognitionHandler :
    ILocalEventHandler<HallEntryConfirmedDomainEvent>,
    ITransientDependency
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRevenueRecognitionService _revenueRecognitionService;

    public RevenueRecognitionHandler(
        IReservationRepository reservationRepository,
        IRevenueRecognitionService revenueRecognitionService)
    {
        _reservationRepository = reservationRepository;
        _revenueRecognitionService = revenueRecognitionService;
    }

    public async Task HandleEventAsync(HallEntryConfirmedDomainEvent eventData)
    {
        var reservation = await _reservationRepository.GetAsync(
            eventData.Snapshot.ReservationId,
            includeDetails: true);

        await _revenueRecognitionService.RecognizeRevenueAsync(reservation);
    }
}
