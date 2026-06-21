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
    public Money TotalPrice { get; set; } = Money.Zero;
    public Money PaidAmount { get; set; } = Money.Zero;

    public string ReservationNumber { get; private set; } = null!;

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public string? CancellationReason { get; private set; }

    public CancellationType? CancellationType { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public ICollection<ReservationService> Services { get; set; } = new List<ReservationService>();

    public static Reservation Create(
        Guid id,
        Guid hallId,
        Guid customerId,
        TimeSlot timeSlot,
        int guestsCount)
    {
        Check.NotNull(timeSlot, nameof(timeSlot));

        if (guestsCount <= 0)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.ReservationGuestsCountInvalid);
        }

        return new Reservation(id)
        {
            HallId = hallId,
            CustomerId = customerId,
            EventDate = timeSlot.EventDate,
            StartTime = timeSlot.StartTime,
            EndTime = timeSlot.EndTime,
            GuestsCount = guestsCount,
            TotalPrice = Money.Zero,
            PaidAmount = Money.Zero,
            Status = ReservationStatus.Pending,
        };
    }

    public TimeSlot GetTimeSlot() => new(EventDate, StartTime, EndTime);

    public void AssignReservationNumber(string reservationNumber)
    {
        ReservationNumber = ReservationCode.Create(reservationNumber);
    }

    public void FinalizeCreation(decimal totalPrice)
    {
        if (totalPrice < 0)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.PaymentAmountInvalid);
        }

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

    public static decimal CalculateTotalPrice(
        decimal hallPricePerHour,
        TimeSlot timeSlot,
        IEnumerable<decimal> servicePrices)
    {
        Check.NotNull(timeSlot, nameof(timeSlot));
        return CalculateTotalPrice(hallPricePerHour, timeSlot.StartTime, timeSlot.EndTime, servicePrices);
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
        if (guestsCount <= 0)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.ReservationGuestsCountInvalid);
        }

        _ = new TimeSlot(eventDate, startTime, endTime);

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
            Status == ReservationStatus.Completed ||
            Status == ReservationStatus.Archived)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotCancel);
        }

        if (cancellationType == Enums.CancellationType.NonPaymentAutoCancel)
        {
            if (Status != ReservationStatus.Confirmed)
            {
                throw new BusinessException(
                    BanquetHallManagementDomainErrorCodes.ReservationCannotCancel);
            }

            if (PaidAmount > 0)
            {
                throw new BusinessException(
                    BanquetHallManagementDomainErrorCodes.ReservationCannotAutoCancelWithPayments);
            }
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

    public void ConfirmHallEntry(DateTime completedAt)
    {
        if (Status == ReservationStatus.Completed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotConfirmHallEntry);
        }

        if (Status != ReservationStatus.FullyPaid)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotConfirmHallEntry);
        }

        if (EventDate.Date != completedAt.Date)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationHallEntryEventDateMismatch);
        }

        AddLocalEvent(new HallEntryConfirmedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));

        CompleteReservation(completedAt);
    }

    public bool CanConfirmHallEntry(DateTime now)
    {
        return Status == ReservationStatus.FullyPaid &&
               EventDate.Date == now.Date;
    }

    public void CompleteReservation(DateTime completedAt)
    {
        EnsureCanComplete();

        Status = ReservationStatus.Completed;
        CompletedAt = completedAt;
        AddLocalEvent(new ReservationCompletedDomainEvent(
            ReservationEventSnapshot.FromReservation(this)));
    }

    public void Complete(DateTime completedAt)
    {
        CompleteReservation(completedAt);
    }

    private void EnsureCanComplete()
    {
        if (Status == ReservationStatus.Completed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
        }

        if (Status != ReservationStatus.FullyPaid)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
        }

        if (PaidAmount < TotalPrice)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
        }
    }

    public bool CanBeUpdated()
    {
        return Status is ReservationStatus.Pending or ReservationStatus.Confirmed;
    }

    public bool CanBeDeleted()
    {
        return Status == ReservationStatus.Pending;
    }

    public bool CanBeArchived()
    {
        return Status is ReservationStatus.Pending
            or ReservationStatus.Confirmed
            or ReservationStatus.Cancelled
            or ReservationStatus.FullyPaid;
    }

    public void Archive()
    {
        if (!CanBeArchived())
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotArchive);
        }

        Status = ReservationStatus.Archived;
    }

    public DateTime GetEventEndDateTime()
    {
        return GetTimeSlot().GetEventEndDateTime();
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
        return ProposedScheduleOverlaps(
            HallId,
            EventDate,
            StartTime,
            EndTime,
            other);
    }

    public static bool ProposedScheduleOverlaps(
        Guid hallId,
        DateTime eventDate,
        TimeSpan startTime,
        TimeSpan endTime,
        Reservation existing)
    {
        if (hallId != existing.HallId)
        {
            return false;
        }

        return eventDate.Date == existing.EventDate.Date
               && startTime < existing.EndTime
               && endTime > existing.StartTime;
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
