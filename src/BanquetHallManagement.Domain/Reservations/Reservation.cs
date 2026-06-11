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
    public decimal PaidAmount { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public string? CancellationReason { get; private set; }

    public CancellationType? CancellationType { get; private set; }

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

    public void ApplyDeposit(decimal amount)
    {
        ApplyPayment(amount);
    }

    public void ApplyInstallment(decimal amount)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentInvalidReservationStatus);
        }

        ApplyPayment(amount);
    }

    public decimal GetRemainingAmount()
    {
        return Math.Max(0m, TotalPrice - PaidAmount);
    }

    public bool TryMarkFullyPaid()
    {
        if (Status != ReservationStatus.Confirmed || PaidAmount < TotalPrice)
        {
            return false;
        }

        MarkFullyPaid();
        return true;
    }

    private void ApplyPayment(decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentAmountInvalid);
        }

        PaidAmount += amount;
    }

    public void Cancel()
    {
        CancelWithReason(Enums.CancellationType.Manual);
    }

    public void CancelWithReason(
        CancellationType cancellationType,
        string? cancellationReason = null)
    {
        if (Status == ReservationStatus.Cancelled ||
            Status == ReservationStatus.Completed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotCancel);
        }

        CancellationType = cancellationType;
        CancellationReason = cancellationReason;
        Status = ReservationStatus.Cancelled;
        AddLocalEvent(new ReservationCancelledDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public void MarkFullyPaid()
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotMarkFullyPaid);
        }

        if (PaidAmount < TotalPrice)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotMarkFullyPaid);
        }

        Status = ReservationStatus.FullyPaid;
        AddLocalEvent(new ReservationFullyPaidDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public void ConfirmHallEntry()
    {
        if (Status != ReservationStatus.FullyPaid)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotConfirmHallEntry);
        }

        Status = ReservationStatus.Completed;
        AddLocalEvent(new HallEntryConfirmedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
        AddLocalEvent(new ReservationCompletedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public void Complete()
    {
        ConfirmHallEntry();
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
        if (Status is not (ReservationStatus.Confirmed or ReservationStatus.FullyPaid))
        {
            return false;
        }

        return GetEventEndDateTime() > asOf;
    }

    public bool OverlapsSchedulingWith(Reservation other)
    {
        if (HallId != other.HallId)
        {
            return false;
        }

        return EventDate.Date == other.EventDate.Date
               && StartTime < other.EndTime
               && EndTime > other.StartTime;
    }

    public bool HasSchedulingConflictWith(
        Reservation other,
        DateTime asOf)
    {
        if (Id == other.Id)
        {
            return false;
        }

        if (!OverlapsSchedulingWith(other))
        {
            return false;
        }

        return other.BlocksScheduling(asOf);
    }
}
