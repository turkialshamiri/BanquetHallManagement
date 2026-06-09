using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Reservations;

public interface IReservationRepository : IRepository<Reservation, Guid>
{
    /// <summary>
    /// Acquires an exclusive transaction-scoped lock for the hall/date scheduling bucket.
    /// Must be called inside an active unit of work with an open transaction.
    /// </summary>
    Task AcquireExclusiveSchedulingLockAsync(
        Guid hallId,
        DateTime eventDate,
        CancellationToken cancellationToken = default);
}
