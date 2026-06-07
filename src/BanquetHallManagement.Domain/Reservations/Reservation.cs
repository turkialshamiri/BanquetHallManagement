using System;
using System.Collections.Generic;
using BanquetHallManagement.Enums;
using BanquetHallManagement.ReservationServices;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Reservations;

public class Reservation : FullAuditedAggregateRoot<Guid>
{
    public Guid HallId { get; set; }
    public Guid CustomerId { get; set; }

    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int GuestsCount { get; set; }
    public decimal TotalPrice { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public ICollection<ReservationService> Services { get; set; } = new List<ReservationService>();

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotConfirm);
        }

        Status = ReservationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled ||
            Status == ReservationStatus.Completed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotCancel);
        }

        Status = ReservationStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
        }

        Status = ReservationStatus.Completed;
    }

    public bool CanBeUpdated()
    {
        return Status is ReservationStatus.Pending or ReservationStatus.Confirmed;
    }

    public bool CanBeDeleted()
    {
        return Status == ReservationStatus.Pending;
    }

    public DateTime GetEventEndDateTime()
    {
        return EventDate.Date + EndTime;
    }

    public bool BlocksScheduling(DateTime asOf)
    {
        if (Status is ReservationStatus.Cancelled or ReservationStatus.Completed)
        {
            return false;
        }

        return GetEventEndDateTime() > asOf;
    }

    public bool HasSchedulingConflictWith(
        Reservation other,
        DateTime asOf)
    {
        if (Id == other.Id)
        {
            return false;
        }

        if (HallId != other.HallId)
        {
            return false;
        }

        if (!BlocksScheduling(asOf) || !other.BlocksScheduling(asOf))
        {
            return false;
        }

        return EventDate.Date == other.EventDate.Date
               && StartTime < other.EndTime
               && EndTime > other.StartTime;
    }
}
