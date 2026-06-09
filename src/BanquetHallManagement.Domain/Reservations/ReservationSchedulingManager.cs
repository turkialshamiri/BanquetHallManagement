using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Reservations;

/// <summary>
/// Serializes and validates hall scheduling to prevent overlapping reservations.
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
        Guid? excludeReservationId = null)
    {
        await _reservationRepository.AcquireExclusiveSchedulingLockAsync(hallId, eventDate);

        var now = Clock.Now;
        var query = await _reservationRepository.GetQueryableAsync();

        var candidate = new Reservation
        {
            HallId = hallId,
            EventDate = eventDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = ReservationStatus.Pending,
        };

        var existingReservations = await AsyncExecuter.ToListAsync(
            query.Where(r =>
                r.HallId == hallId &&
                r.EventDate.Date == eventDate.Date &&
                (!excludeReservationId.HasValue || r.Id != excludeReservationId.Value)));

        var hasConflict = existingReservations.Any(existing =>
            candidate.HasSchedulingConflictWith(existing, now));

        if (hasConflict)
        {
            throw new UserFriendlyException(
                "هذه القاعة محجوزة بالفعل في هذا الوقت");
        }
    }
}
