using BanquetHallManagement.Events.Reservations;
using Microsoft.Extensions.Logging;

namespace BanquetHallManagement.EventHandlers.Reservations;

internal static class ReservationLifecycleEventLogger
{
    public static void Log(
        ILogger logger,
        string lifecycleAction,
        ReservationEventEto eventData)
    {
        logger.LogInformation(
            "Reservation {LifecycleAction}: {ReservationId}, Hall: {HallName}, Customer: {CustomerName}, " +
            "TotalPrice: {TotalPrice}, Status: {ReservationStatus}, UserId: {UserId}, UserName: {UserName}, " +
            "Timestamp: {Timestamp}",
            lifecycleAction,
            eventData.ReservationId,
            eventData.HallName,
            eventData.CustomerName,
            eventData.TotalPrice,
            eventData.ReservationStatus,
            eventData.UserId,
            eventData.UserName,
            eventData.Timestamp);
    }
}
