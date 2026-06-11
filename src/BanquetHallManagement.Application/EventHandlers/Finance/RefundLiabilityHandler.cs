using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Refunds;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class RefundLiabilityHandler :
    ILocalEventHandler<ReservationCancelledDomainEvent>,
    ITransientDependency
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRefundLiabilityService _refundLiabilityService;

    public RefundLiabilityHandler(
        IReservationRepository reservationRepository,
        IRefundLiabilityService refundLiabilityService)
    {
        _reservationRepository = reservationRepository;
        _refundLiabilityService = refundLiabilityService;
    }

    public async Task HandleEventAsync(ReservationCancelledDomainEvent eventData)
    {
        if (eventData.Snapshot.ReservationStatus != ReservationStatus.Cancelled)
        {
            return;
        }

        var reservation = await _reservationRepository.GetAsync(
            eventData.Snapshot.ReservationId);

        if (reservation.CancellationType != CancellationType.NonPaymentAutoCancel)
        {
            return;
        }

        await _refundLiabilityService.TransferInstallmentsToLiabilityAsync(reservation);
    }
}
