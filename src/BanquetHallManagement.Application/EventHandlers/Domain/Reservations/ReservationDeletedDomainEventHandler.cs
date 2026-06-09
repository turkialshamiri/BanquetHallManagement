using System;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.EventHandlers.Domain.Reservations;

public class ReservationDeletedDomainEventHandler :
    ReservationDomainEventHandlerBase<ReservationDeletedDomainEvent>
{
    public ReservationDeletedDomainEventHandler(
        IRepository<Hall, Guid> hallRepository,
        IRepository<Customer, Guid> customerRepository,
        ReservationDistributedEventPublisher publisher,
        ILogger<ReservationDeletedDomainEventHandler> logger)
        : base(hallRepository, customerRepository, publisher, logger)
    {
    }

    protected override Task PublishDistributedEventsAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return Publisher.PublishDeletedAsync(snapshot, hall, customer);
    }
}
