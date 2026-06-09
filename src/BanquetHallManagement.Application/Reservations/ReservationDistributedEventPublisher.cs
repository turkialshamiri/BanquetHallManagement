using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.EventHandlers.Domain.Reservations;
using BanquetHallManagement.Events.Audit;
using BanquetHallManagement.Events.Notifications;
using BanquetHallManagement.Events.Reservations;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace BanquetHallManagement.Reservations;

public class ReservationDistributedEventPublisher : ITransientDependency
{
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly ReservationEventMapper _reservationEventMapper;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<ReservationDistributedEventPublisher> _logger;

    public ReservationDistributedEventPublisher(
        IDistributedEventBus distributedEventBus,
        ReservationEventMapper reservationEventMapper,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<ReservationDistributedEventPublisher> logger)
    {
        _distributedEventBus = distributedEventBus;
        _reservationEventMapper = reservationEventMapper;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public Task PublishCreatedAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return PublishLifecycleAsync(
            _reservationEventMapper.MapToCreatedEto(snapshot, hall, customer),
            snapshot,
            ReservationAuditActions.Created,
            hall,
            customer,
            ReservationNotificationTypes.Created,
            $"تم إنشاء حجز جديد للقاعة {hall.Name} بتاريخ {snapshot.EventDate:yyyy-MM-dd}");
    }

    public Task PublishUpdatedAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return PublishLifecycleAsync(
            _reservationEventMapper.MapToUpdatedEto(snapshot, hall, customer),
            snapshot,
            ReservationAuditActions.Updated,
            hall,
            customer);
    }

    public Task PublishDeletedAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return PublishLifecycleAsync(
            _reservationEventMapper.MapToDeletedEto(snapshot, hall, customer),
            snapshot,
            ReservationAuditActions.Deleted,
            hall,
            customer);
    }

    public Task PublishConfirmedAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return PublishLifecycleAsync(
            _reservationEventMapper.MapToConfirmedEto(snapshot, hall, customer),
            snapshot,
            ReservationAuditActions.Confirmed,
            hall,
            customer,
            ReservationNotificationTypes.Confirmed,
            $"تم تأكيد حجز القاعة {hall.Name} بتاريخ {snapshot.EventDate:yyyy-MM-dd}");
    }

    public Task PublishCancelledAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return PublishLifecycleAsync(
            _reservationEventMapper.MapToCancelledEto(snapshot, hall, customer),
            snapshot,
            ReservationAuditActions.Cancelled,
            hall,
            customer,
            ReservationNotificationTypes.Cancelled,
            $"تم إلغاء حجز القاعة {hall.Name} بتاريخ {snapshot.EventDate:yyyy-MM-dd}");
    }

    public Task PublishCompletedAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return PublishLifecycleAsync(
            _reservationEventMapper.MapToCompletedEto(snapshot, hall, customer),
            snapshot,
            ReservationAuditActions.Completed,
            hall,
            customer,
            ReservationNotificationTypes.Completed,
            $"تم إكمال حجز القاعة {hall.Name} بتاريخ {snapshot.EventDate:yyyy-MM-dd}");
    }

    private async Task PublishLifecycleAsync(
        ReservationEventEto lifecycleEto,
        ReservationEventSnapshot snapshot,
        string auditAction,
        Hall hall,
        Customer customer,
        string? notificationType = null,
        string? notificationMessage = null)
    {
        ReservationDomainEventDiagnostics.LogDistributedEventPublished(
            _logger,
            lifecycleEto.GetType().Name,
            snapshot,
            lifecycleEto.UserId,
            lifecycleEto.UserName,
            lifecycleEto.Timestamp);

        await _distributedEventBus.PublishAsync(lifecycleEto);

        await _distributedEventBus.PublishAsync(
            CreateAuditEto(auditAction, snapshot, hall, customer));

        if (notificationType != null && notificationMessage != null)
        {
            await _distributedEventBus.PublishAsync(
                CreateNotificationEto(notificationType, notificationMessage, snapshot, hall, customer));
        }
    }

    private ReservationAuditEto CreateAuditEto(
        string action,
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return new ReservationAuditEto
        {
            ReservationId = snapshot.ReservationId,
            Action = action,
            Timestamp = _clock.Now,
            UserId = _currentUser.Id,
            UserName = _currentUser.UserName ?? string.Empty,
            HallId = snapshot.HallId,
            HallName = hall.Name ?? string.Empty,
            CustomerId = snapshot.CustomerId,
            CustomerName = customer.Name ?? string.Empty,
            ReservationStatus = snapshot.ReservationStatus,
        };
    }

    private static ReservationNotificationEto CreateNotificationEto(
        string notificationType,
        string message,
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return new ReservationNotificationEto
        {
            ReservationId = snapshot.ReservationId,
            CustomerId = snapshot.CustomerId,
            CustomerName = customer.Name ?? string.Empty,
            HallName = hall.Name ?? string.Empty,
            EventDate = snapshot.EventDate,
            NotificationType = notificationType,
            Message = message,
        };
    }
}
