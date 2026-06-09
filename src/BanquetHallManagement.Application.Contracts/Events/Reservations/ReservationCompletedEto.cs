using BanquetHallManagement.Events;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.Events.Reservations;

[EventName(BanquetHallManagementEventNames.ReservationCompleted)]
public class ReservationCompletedEto : ReservationEventEto
{
}
