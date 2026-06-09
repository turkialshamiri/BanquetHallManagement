using BanquetHallManagement.Events;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.Events.Reservations;

[EventName(BanquetHallManagementEventNames.ReservationCreated)]
public class ReservationCreatedEto : ReservationEventEto
{
}
