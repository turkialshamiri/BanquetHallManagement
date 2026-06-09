using System;
using System.Collections.Generic;
using System.Linq;
using BanquetHallManagement.Enums;
using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Reservations.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace BanquetHallManagement.Reservations;

public class Reservation : FullAuditedAggregateRoot<Guid>
{
    internal Reservation()
    {
    }

    public Reservation(Guid id)
        : base(id)
    {
    }

    public Guid HallId { get; set; }
    public Guid CustomerId { get; set; }

    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int GuestsCount { get; set; }
    public decimal TotalPrice { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public ICollection<ReservationService> Services { get; set; } = new List<ReservationService>();

    public void FinalizeCreation(decimal totalPrice)
    {
        TotalPrice = totalPrice;
        AddLocalEvent(new ReservationCreatedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public static decimal CalculateTotalPrice(
        decimal hallPricePerHour,
        TimeSpan startTime,
        TimeSpan endTime,
        IEnumerable<decimal> servicePrices)
    {
        var reservationHours = (endTime - startTime).TotalHours;
        return (decimal)reservationHours * hallPricePerHour + servicePrices.Sum();
    }

    public void ApplyUpdate(
        Guid hallId,
        Guid customerId,
        DateTime eventDate,
        TimeSpan startTime,
        TimeSpan endTime,
        int guestsCount,
        decimal totalPrice)
    {
        HallId = hallId;
        CustomerId = customerId;
        EventDate = eventDate;
        StartTime = startTime;
        EndTime = endTime;
        GuestsCount = guestsCount;
        TotalPrice = totalPrice;
        AddLocalEvent(new ReservationUpdatedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public void MarkForDeletion()
    {
        AddLocalEvent(new ReservationDeletedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotConfirm);
        }

        Status = ReservationStatus.Confirmed;
        AddLocalEvent(new ReservationConfirmedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
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
        AddLocalEvent(new ReservationCancelledDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public void Complete()
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
        }

        Status = ReservationStatus.Completed;
        AddLocalEvent(new ReservationCompletedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
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
