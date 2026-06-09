using System;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Domain.Reservations;

public abstract class ReservationDomainEventHandlerBase<TEvent> :
    ILocalEventHandler<TEvent>,
    ITransientDependency
    where TEvent : class, IReservationDomainEvent
{
    private readonly ILogger _logger;

    protected ReservationDomainEventHandlerBase(
        IRepository<Hall, Guid> hallRepository,
        IRepository<Customer, Guid> customerRepository,
        ReservationDistributedEventPublisher publisher,
        ILogger logger)
    {
        HallRepository = hallRepository;
        CustomerRepository = customerRepository;
        Publisher = publisher;
        _logger = logger;
    }

    protected IRepository<Hall, Guid> HallRepository { get; }

    protected IRepository<Customer, Guid> CustomerRepository { get; }

    protected ReservationDistributedEventPublisher Publisher { get; }

    public async Task HandleEventAsync(TEvent eventData)
    {
        var eventType = typeof(TEvent).Name;

        ReservationDomainEventDiagnostics.LogDomainEventRaised(_logger, eventType, eventData);

        var hall = await HallRepository.GetAsync(eventData.Snapshot.HallId);
        var customer = await CustomerRepository.GetAsync(eventData.Snapshot.CustomerId);

        await PublishDistributedEventsAsync(eventData.Snapshot, hall, customer);

        ReservationDomainEventDiagnostics.LogHandlerExecuted(
            _logger,
            GetType().Name,
            eventType,
            eventData.Snapshot.ReservationId);
    }

    protected abstract Task PublishDistributedEventsAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer);
}
