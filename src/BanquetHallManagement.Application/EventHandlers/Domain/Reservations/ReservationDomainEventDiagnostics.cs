using System;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;

namespace BanquetHallManagement.EventHandlers.Domain.Reservations;

internal static class ReservationDomainEventDiagnostics
{
    public static void LogDomainEventRaised(
        ILogger logger,
        string eventType,
        IReservationDomainEvent domainEvent)
    {
        logger.LogInformation(
            "Domain event raised: {EventType}, ReservationId: {ReservationId}, Status: {ReservationStatus}",
            eventType,
            domainEvent.Snapshot.ReservationId,
            domainEvent.Snapshot.ReservationStatus);
    }

    public static void LogDistributedEventPublished(
        ILogger logger,
        string eventType,
        ReservationEventSnapshot snapshot,
        Guid? userId,
        string userName,
        DateTime timestamp)
    {
        logger.LogInformation(
            "Distributed event published: {EventType}, ReservationId: {ReservationId}, UserId: {UserId}, " +
            "UserName: {UserName}, Timestamp: {Timestamp}",
            eventType,
            snapshot.ReservationId,
            userId,
            userName,
            timestamp);
    }

    public static void LogHandlerExecuted(
        ILogger logger,
        string handlerName,
        string eventType,
        Guid reservationId)
    {
        logger.LogInformation(
            "Domain event handler executed: {HandlerName}, EventType: {EventType}, ReservationId: {ReservationId}",
            handlerName,
            eventType,
            reservationId);
    }
}
