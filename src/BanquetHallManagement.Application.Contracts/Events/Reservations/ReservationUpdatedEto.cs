using BanquetHallManagement.Events;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.Events.Reservations;

[EventName(BanquetHallManagementEventNames.ReservationUpdated)]
public class ReservationUpdatedEto : ReservationEventEto
{
}
