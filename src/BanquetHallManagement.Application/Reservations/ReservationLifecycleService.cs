using System;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Halls;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Reservations;

public class ReservationLifecycleService : ITransientDependency
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly HallAvailabilityManager _hallAvailabilityManager;
    private readonly ReservationSchedulingManager _reservationSchedulingManager;
    private readonly IHallAccessCardRepository _hallAccessCardRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ReservationLifecycleService(
        IReservationRepository reservationRepository,
        IRepository<Hall, Guid> hallRepository,
        HallAvailabilityManager hallAvailabilityManager,
        ReservationSchedulingManager reservationSchedulingManager,
        IHallAccessCardRepository hallAccessCardRepository,
        IClock clock,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _reservationRepository = reservationRepository;
        _hallRepository = hallRepository;
        _hallAvailabilityManager = hallAvailabilityManager;
        _reservationSchedulingManager = reservationSchedulingManager;
        _hallAccessCardRepository = hallAccessCardRepository;
        _clock = clock;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task DeleteAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        if (!reservation.CanBeDeleted())
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotDelete);
        }

        reservation.MarkForDeletion();

        await _reservationRepository.DeleteAsync(reservation);
    }

    public async Task<Reservation> ArchiveAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);

        reservation.Archive();

        await _reservationRepository.UpdateAsync(reservation, autoSave: false);
        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return reservation;
    }

    public async Task<Reservation> CancelAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);

        reservation.Cancel();

        await _reservationRepository.UpdateAsync(reservation, autoSave: false);
        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return reservation;
    }

    public async Task<Reservation> ConfirmAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        await _reservationSchedulingManager.EnsureNoSchedulingConflictAsync(
            reservation.HallId,
            reservation.EventDate,
            reservation.StartTime,
            reservation.EndTime,
            reservation.Id,
            ReservationStatus.Confirmed);

        reservation.Confirm();

        await _reservationRepository.UpdateAsync(reservation, autoSave: false);

        await _reservationSchedulingManager.CancelConflictingPendingAsync(reservation.Id);

        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return reservation;
    }

    public async Task<Reservation> ConfirmHallEntryAsync(Reservation reservation)
    {
        await EnsureHallAccessCardExistsAsync(reservation.Id);
        reservation.ConfirmHallEntry(_clock.Now);

        await _reservationRepository.UpdateAsync(reservation, autoSave: false);
        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return reservation;
    }

    public async Task<Reservation> CompleteAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);

        reservation.CompleteReservation(_clock.Now);

        await _reservationRepository.UpdateAsync(reservation, autoSave: false);
        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return reservation;
    }

    public async Task<Reservation> GetByReservationNumberOrThrowAsync(string reservationNumber)
    {
        var reservation = await _reservationRepository.FindByReservationNumberAsync(reservationNumber);
        if (reservation == null)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.ReservationNotFound)
                .WithData("ReservationNumber", reservationNumber);
        }

        return reservation;
    }

    private async Task EnsureHallAccessCardExistsAsync(Guid reservationId)
    {
        var card = await _hallAccessCardRepository.FindByReservationIdAsync(reservationId);
        if (card == null)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.HallAccessCardNotFound)
                .WithData("ReservationId", reservationId);
        }
    }
}
