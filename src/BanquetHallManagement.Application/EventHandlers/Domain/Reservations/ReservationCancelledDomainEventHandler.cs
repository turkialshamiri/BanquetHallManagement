using System;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;

using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.EventHandlers.Domain.Reservations;

public class ReservationCancelledDomainEventHandler :
    ReservationDomainEventHandlerBase<ReservationCancelledDomainEvent>
{
    public ReservationCancelledDomainEventHandler(
        IRepository<Hall, Guid> hallRepository,
        IRepository<Customer, Guid> customerRepository,
        ReservationDistributedEventPublisher publisher,
        ILogger<ReservationCancelledDomainEventHandler> logger)
        : base(hallRepository, customerRepository, publisher, logger)
    {
    }

    protected override Task PublishDistributedEventsAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return Publisher.PublishCancelledAsync(snapshot, hall, customer);
    }
}
