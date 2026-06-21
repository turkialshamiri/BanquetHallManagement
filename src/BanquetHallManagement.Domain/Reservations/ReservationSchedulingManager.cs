using System;
using System.Collections.Generic;
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

        var existingReservations = await _reservationRepository.GetListByHallAndEventDateAsync(
            hallId,
            eventDate,
            excludeReservationId,
            cancellationToken);

        var hasConflict = existingReservations.Any(existing =>
            existing.BlocksScheduling(now) &&
            Reservation.ProposedScheduleOverlaps(
                hallId,
                eventDate,
                startTime,
                endTime,
                existing));

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

        var pendingReservations = await _reservationRepository.GetPendingByHallAndEventDateAsync(
            confirmedReservation.HallId,
            confirmedReservation.EventDate,
            confirmedReservationId,
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
