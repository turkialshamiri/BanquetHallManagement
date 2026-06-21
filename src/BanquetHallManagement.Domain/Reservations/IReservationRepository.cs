using System;
using System.Collections.Generic;
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

    Task<Reservation?> FindByReservationNumberAsync(
        string reservationNumber,
        CancellationToken cancellationToken = default);

    Task<List<Reservation>> GetListByHallAndEventDateAsync(
        Guid hallId,
        DateTime eventDate,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default);

    Task<List<Reservation>> GetPendingByHallAndEventDateAsync(
        Guid hallId,
        DateTime eventDate,
        Guid excludeReservationId,
        CancellationToken cancellationToken = default);

    Task<List<Reservation>> GetConfirmedWithoutPaymentsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> IsServiceReferencedAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task SyncReservationServicesAsync(
        Guid reservationId,
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default);
}
