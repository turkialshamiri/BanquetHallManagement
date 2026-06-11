using System.Threading.Tasks;
using BanquetHallManagement.Reservations.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class FullyPaidHandler :
    ILocalEventHandler<ReservationFullyPaidDomainEvent>,
    ITransientDependency
{
    private readonly ILogger<FullyPaidHandler> _logger;

    public FullyPaidHandler(ILogger<FullyPaidHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ReservationFullyPaidDomainEvent eventData)
    {
        var snapshot = eventData.Snapshot;

        _logger.LogInformation(
            "Reservation fully paid | Reservation: {ReservationId} | PaidAmount: {PaidAmount} | TotalPrice: {TotalPrice}",
            snapshot.ReservationId,
            snapshot.PaidAmount,
            snapshot.TotalPrice);

        return Task.CompletedTask;
    }
}
