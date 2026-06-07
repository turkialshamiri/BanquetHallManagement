using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Reservations;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Halls;

public class HallAvailabilityManager : DomainService
{
    public HallStatus GetOperationalStatus(Hall hall)
    {
        if (hall.Status == HallStatus.Maintenance)
        {
            return HallStatus.Maintenance;
        }

        return HallStatus.Available;
    }

    public HallStatus ResolveEffectiveStatus(Hall hall, bool hasActiveConfirmedReservation)
    {
        if (hall.Status == HallStatus.Maintenance)
        {
            return HallStatus.Maintenance;
        }

        if (hasActiveConfirmedReservation)
        {
            return HallStatus.Booked;
        }

        return HallStatus.Available;
    }

    public bool IsActiveConfirmedReservation(Reservation reservation, DateTime asOf)
    {
        return reservation.Status == ReservationStatus.Confirmed
               && reservation.GetEventEndDateTime() > asOf;
    }

    public void EnsureCanAcceptBookings(Hall hall)
    {
        if (hall.Status == HallStatus.Maintenance)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.HallUnderMaintenance);
        }
    }

    public void EnsureOperationalStatus(HallStatus status)
    {
        if (status == HallStatus.Booked)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.HallStatusCannotBeBooked);
        }

        if (status != HallStatus.Available && status != HallStatus.Maintenance)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.HallStatusInvalid);
        }
    }
}
