using System;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.EventHandlers.Domain.Reservations;

public class ReservationUpdatedDomainEventHandler :
    ReservationDomainEventHandlerBase<ReservationUpdatedDomainEvent>
{
    public ReservationUpdatedDomainEventHandler(
        IRepository<Hall, Guid> hallRepository,
        IRepository<Customer, Guid> customerRepository,
        ReservationDistributedEventPublisher publisher,
        ILogger<ReservationUpdatedDomainEventHandler> logger)
        : base(hallRepository, customerRepository, publisher, logger)
    {
    }

    protected override Task PublishDistributedEventsAsync(
        ReservationEventSnapshot snapshot,
        Hall hall,
        Customer customer)
    {
        return Publisher.PublishUpdatedAsync(snapshot, hall, customer);
    }
}
