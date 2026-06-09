using BanquetHallManagement.Events;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.Events.Reservations;

[EventName(BanquetHallManagementEventNames.ReservationCancelled)]
public class ReservationCancelledEto : ReservationEventEto
{
}
