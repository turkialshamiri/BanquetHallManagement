using BanquetHallManagement.Events;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.Events.Reservations;

[EventName(BanquetHallManagementEventNames.ReservationConfirmed)]
public class ReservationConfirmedEto : ReservationEventEto
{
}
