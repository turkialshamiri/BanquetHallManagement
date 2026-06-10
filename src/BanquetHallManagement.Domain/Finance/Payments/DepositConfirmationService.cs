using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Payments;

public class DepositConfirmationService : DomainService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly HallAvailabilityManager _hallAvailabilityManager;
    private readonly ReservationSchedulingManager _reservationSchedulingManager;

    public DepositConfirmationService(
        IReservationRepository reservationRepository,
        IRepository<Hall, Guid> hallRepository,
        HallAvailabilityManager hallAvailabilityManager,
        ReservationSchedulingManager reservationSchedulingManager)
    {
        _reservationRepository = reservationRepository;
        _hallRepository = hallRepository;
        _hallAvailabilityManager = hallAvailabilityManager;
        _reservationSchedulingManager = reservationSchedulingManager;
    }

    public async Task ConfirmWithDepositAsync(
        Reservation reservation,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var hall = await _hallRepository.GetAsync(reservation.HallId, cancellationToken: cancellationToken);
        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        await _reservationSchedulingManager.EnsureNoSchedulingConflictAsync(
            reservation.HallId,
            reservation.EventDate,
            reservation.StartTime,
            reservation.EndTime,
            reservation.Id,
            ReservationStatus.Confirmed,
            cancellationToken);

        reservation.Confirm();
        reservation.ApplyDeposit(amount);

        await _reservationRepository.UpdateAsync(reservation, autoSave: false, cancellationToken: cancellationToken);

        await _reservationSchedulingManager.CancelConflictingPendingAsync(
            reservation.Id,
            cancellationToken);
    }
}
