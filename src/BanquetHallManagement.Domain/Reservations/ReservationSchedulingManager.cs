using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Reservations;

/// <summary>
/// Serializes and validates hall scheduling to prevent overlapping blocking reservations.
/// Only Confirmed and FullyPaid reservations block the schedule; Pending overlaps are allowed.
/// </summary>
public class ReservationSchedulingManager : DomainService
{
    private readonly IReservationRepository _reservationRepository;

    public ReservationSchedulingManager(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task EnsureNoSchedulingConflictAsync(
        Guid hallId,
        DateTime eventDate,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeReservationId = null,
        ReservationStatus candidateStatus = ReservationStatus.Pending,
        CancellationToken cancellationToken = default)
    {
        await _reservationRepository.AcquireExclusiveSchedulingLockAsync(
            hallId,
            eventDate,
            cancellationToken);

        var now = Clock.Now;
        var query = await _reservationRepository.GetQueryableAsync();

        var candidate = new Reservation
        {
            HallId = hallId,
            EventDate = eventDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = candidateStatus,
        };

        var existingReservations = await AsyncExecuter.ToListAsync(
            query.Where(r =>
                r.HallId == hallId &&
                r.EventDate.Date == eventDate.Date &&
                (!excludeReservationId.HasValue || r.Id != excludeReservationId.Value)),
            cancellationToken);

        var hasConflict = existingReservations.Any(existing =>
            candidate.HasSchedulingConflictWith(existing, now));

        if (hasConflict)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationSchedulingConflict);
        }
    }

    public async Task CancelConflictingPendingAsync(
        Guid confirmedReservationId,
        CancellationToken cancellationToken = default)
    {
        var confirmedReservation = await _reservationRepository.GetAsync(
            confirmedReservationId,
            cancellationToken: cancellationToken);

        if (confirmedReservation.Status is not (
            ReservationStatus.Confirmed or ReservationStatus.FullyPaid))
        {
            return;
        }

        var query = await _reservationRepository.GetQueryableAsync();

        var pendingReservations = await AsyncExecuter.ToListAsync(
            query.Where(r =>
                r.Id != confirmedReservationId &&
                r.HallId == confirmedReservation.HallId &&
                r.EventDate.Date == confirmedReservation.EventDate.Date &&
                r.Status == ReservationStatus.Pending),
            cancellationToken);

        foreach (var pendingReservation in pendingReservations)
        {
            if (!confirmedReservation.OverlapsSchedulingWith(pendingReservation))
            {
                continue;
            }

            pendingReservation.CancelWithReason(
                CancellationType.ConflictOverride,
                $"Cancelled due to conflicting reservation {confirmedReservationId}.");

            await _reservationRepository.UpdateAsync(
                pendingReservation,
                cancellationToken: cancellationToken);
        }
    }
}
