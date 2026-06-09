using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Events.Reservations;
using BanquetHallManagement.Reservations.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace BanquetHallManagement.Reservations;

public class ReservationEventMapper : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public ReservationEventMapper(ICurrentUser currentUser, IClock clock)
    {
        _currentUser = currentUser;
        _clock = clock;
    }

    public ReservationCreatedEto MapToCreatedEto(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return MapBase<ReservationCreatedEto>(snapshot, hall, customer);
    }

    public ReservationUpdatedEto MapToUpdatedEto(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return MapBase<ReservationUpdatedEto>(snapshot, hall, customer);
    }

    public ReservationDeletedEto MapToDeletedEto(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return MapBase<ReservationDeletedEto>(snapshot, hall, customer);
    }

    public ReservationConfirmedEto MapToConfirmedEto(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return MapBase<ReservationConfirmedEto>(snapshot, hall, customer);
    }

    public ReservationCancelledEto MapToCancelledEto(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return MapBase<ReservationCancelledEto>(snapshot, hall, customer);
    }

    public ReservationCompletedEto MapToCompletedEto(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return MapBase<ReservationCompletedEto>(snapshot, hall, customer);
    }

    private TEvent MapBase<TEvent>(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
        where TEvent : ReservationEventEto, new()
    {
        return new TEvent
        {
            ReservationId = snapshot.ReservationId,
            HallId = snapshot.HallId,
            HallName = hall.Name ?? string.Empty,
            CustomerId = snapshot.CustomerId,
            CustomerName = customer.Name ?? string.Empty,
            EventDate = snapshot.EventDate,
            GuestsCount = snapshot.GuestsCount,
            TotalPrice = snapshot.TotalPrice,
            ReservationStatus = snapshot.ReservationStatus,
            UserId = _currentUser.Id,
            UserName = _currentUser.UserName ?? string.Empty,
            Timestamp = _clock.Now,
        };
    }
}
