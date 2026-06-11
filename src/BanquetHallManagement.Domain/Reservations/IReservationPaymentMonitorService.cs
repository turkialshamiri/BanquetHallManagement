using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Reservations;

public interface IReservationPaymentMonitorService : IDomainService
{
    Task ProcessAsync(CancellationToken cancellationToken = default);

    bool IsEligibleForAutoCancel(Reservation reservation, DateTime now);
}
