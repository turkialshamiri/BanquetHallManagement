using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Reservations;

public class ReservationPaymentMonitorService : DomainService, IReservationPaymentMonitorService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ReservationPaymentMonitorService(
        IReservationRepository reservationRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _reservationRepository = reservationRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var now = Clock.Now;
        var candidateIds = await GetEligibleReservationIdsAsync(now, cancellationToken);

        foreach (var candidateId in candidateIds)
        {
            using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);

            var reservation = await _reservationRepository.GetAsync(
                candidateId,
                cancellationToken: cancellationToken);

            if (!IsEligibleForAutoCancel(reservation, now))
            {
                continue;
            }

            reservation.CancelWithReason(
                CancellationType.NonPaymentAutoCancel,
                "Automatic cancellation: payment incomplete within 2 hours of event start.");

            await _reservationRepository.UpdateAsync(reservation, autoSave: true, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
        }
    }

    public bool IsEligibleForAutoCancel(Reservation reservation, DateTime now)
    {
        if (reservation.Status != ReservationStatus.Confirmed)
        {
            return false;
        }

        if (reservation.PaidAmount > 0)
        {
            return false;
        }

        var eventStart = reservation.EventDate.Date.Add(reservation.StartTime);
        var remaining = eventStart - now;

        return remaining > TimeSpan.Zero &&
               remaining < ReservationPaymentMonitorConsts.AutoCancelWindow;
    }

    private async Task<List<Guid>> GetEligibleReservationIdsAsync(
        DateTime now,
        CancellationToken cancellationToken)
    {
        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);

        var confirmed = await _reservationRepository.GetConfirmedWithoutPaymentsAsync(
            cancellationToken);

        var candidateIds = confirmed
            .Where(reservation => IsEligibleForAutoCancel(reservation, now))
            .Select(reservation => reservation.Id)
            .ToList();

        await unitOfWork.CompleteAsync(cancellationToken);

        return candidateIds;
    }
}
