using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace BanquetHallManagement.Reservations;

public class ReservationAggregateTests
{
    private static readonly DateTime AsOf = new(2026, 6, 10, 10, 0, 0);
    private static readonly Guid HallId = Guid.NewGuid();

    [Fact]
    public void Two_Pending_Reservations_Should_Not_Have_Scheduling_Conflict()
    {
        var first = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        var second = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        first.HasSchedulingConflictWith(second, AsOf).ShouldBeFalse();
        first.BlocksScheduling(AsOf).ShouldBeFalse();
        second.BlocksScheduling(AsOf).ShouldBeFalse();
    }

    [Fact]
    public void Confirmed_Reservation_Should_Block_Scheduling()
    {
        var reservation = CreateReservation(
            ReservationStatus.Confirmed,
            new TimeSpan(18, 0, 0),
            new TimeSpan(22, 0, 0),
            eventDate: AsOf.Date);

        reservation.BlocksScheduling(AsOf).ShouldBeTrue();
    }

    [Fact]
    public void FullyPaid_Reservation_Should_Block_Scheduling()
    {
        var reservation = CreateReservation(
            ReservationStatus.FullyPaid,
            new TimeSpan(18, 0, 0),
            new TimeSpan(22, 0, 0),
            eventDate: AsOf.Date);

        reservation.BlocksScheduling(AsOf).ShouldBeTrue();
    }

    [Fact]
    public void Pending_Reservation_Should_Conflict_With_Confirmed_For_Same_Slot()
    {
        var pending = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        var confirmed = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        pending.HasSchedulingConflictWith(confirmed, AsOf).ShouldBeTrue();
    }

    [Fact]
    public void Confirmed_Reservation_Should_Conflict_With_Another_Confirmed_For_Same_Slot()
    {
        var first = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        var second = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(19, 0, 0), new TimeSpan(21, 0, 0));

        first.HasSchedulingConflictWith(second, AsOf).ShouldBeTrue();
    }

    [Fact]
    public void CancelWithReason_Should_Reject_NonPaymentAutoCancel_When_Payments_Recorded()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 100_000m;
        reservation.PaidAmount = 30_000m;

        Should.Throw<Volo.Abp.BusinessException>(() =>
            reservation.CancelWithReason(CancellationType.NonPaymentAutoCancel));
    }

    [Fact]
    public void CancelWithReason_Should_Cancel_Pending_Reservation_With_Reason()
    {
        var reservation = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        reservation.CancelWithReason(CancellationType.Manual, "Customer request");

        reservation.Status.ShouldBe(ReservationStatus.Cancelled);
        reservation.CancellationType.ShouldBe(CancellationType.Manual);
        reservation.CancellationReason.ShouldBe("Customer request");
    }

    [Fact]
    public void Cancel_Should_Use_Manual_Cancellation_Type()
    {
        var reservation = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        reservation.Cancel();

        reservation.Status.ShouldBe(ReservationStatus.Cancelled);
        reservation.CancellationType.ShouldBe(CancellationType.Manual);
    }

    [Fact]
    public void MarkFullyPaid_Should_Move_Confirmed_Reservation_To_FullyPaid()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;

        reservation.MarkFullyPaid();

        reservation.Status.ShouldBe(ReservationStatus.FullyPaid);
    }

    [Fact]
    public void MarkFullyPaid_Should_Throw_When_PaidAmount_Is_Less_Than_Total()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 500m;

        Should.Throw<BusinessException>(() => reservation.MarkFullyPaid())
            .Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationCannotMarkFullyPaid);
    }

    private static readonly DateTime CompletedAt = new(2026, 6, 12, 18, 0, 0);

    [Fact]
    public void ConfirmHallEntry_Should_Require_FullyPaid_Status()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        Should.Throw<BusinessException>(() => reservation.ConfirmHallEntry(CompletedAt))
            .Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationCannotConfirmHallEntry);
    }

    [Fact]
    public void ConfirmHallEntry_Should_Move_FullyPaid_Reservation_To_Completed()
    {
        var reservation = CreateReservation(
            ReservationStatus.FullyPaid,
            new TimeSpan(18, 0, 0),
            new TimeSpan(22, 0, 0),
            eventDate: CompletedAt.Date);
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;

        reservation.ConfirmHallEntry(CompletedAt);

        reservation.Status.ShouldBe(ReservationStatus.Completed);
        reservation.CompletedAt.ShouldBe(CompletedAt);
    }

    [Fact]
    public void ConfirmHallEntry_Should_Require_Event_Date_To_Match_Confirmation_Date()
    {
        var reservation = CreateReservation(ReservationStatus.FullyPaid, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;

        Should.Throw<BusinessException>(() => reservation.ConfirmHallEntry(CompletedAt))
            .Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationHallEntryEventDateMismatch);
    }

    [Fact]
    public void CompleteReservation_Should_Move_FullyPaid_Reservation_To_Completed()
    {
        var reservation = CreateReservation(ReservationStatus.FullyPaid, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;

        reservation.CompleteReservation(CompletedAt);

        reservation.Status.ShouldBe(ReservationStatus.Completed);
        reservation.CompletedAt.ShouldBe(CompletedAt);
    }

    [Fact]
    public void CompleteReservation_Should_Throw_When_Already_Completed()
    {
        var reservation = CreateReservation(ReservationStatus.FullyPaid, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;
        reservation.CompleteReservation(CompletedAt);

        Should.Throw<BusinessException>(() => reservation.CompleteReservation(CompletedAt))
            .Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
    }

    [Fact]
    public void CompleteReservation_Should_Throw_When_Not_Fully_Paid()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;

        Should.Throw<BusinessException>(() => reservation.CompleteReservation(CompletedAt))
            .Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
    }

    [Fact]
    public void Complete_Should_Delegate_To_CompleteReservation()
    {
        var reservation = CreateReservation(ReservationStatus.FullyPaid, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;

        reservation.Complete(CompletedAt);

        reservation.Status.ShouldBe(ReservationStatus.Completed);
        reservation.CompletedAt.ShouldBe(CompletedAt);
    }

    [Fact]
    public void GetRemainingAmount_Should_Return_Outstanding_Balance()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 100_000m;
        reservation.PaidAmount = 30_000m;

        reservation.GetRemainingAmount().ShouldBe(70_000m);
    }

    [Fact]
    public void TryMarkFullyPaid_Should_Transition_Confirmed_To_FullyPaid()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 100_000m;
        reservation.PaidAmount = 100_000m;

        reservation.TryMarkFullyPaid().ShouldBeTrue();
        reservation.Status.ShouldBe(ReservationStatus.FullyPaid);
    }

    [Fact]
    public void ApplyInstallment_Should_Reject_When_Not_Confirmed()
    {
        var reservation = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        Should.Throw<BusinessException>(() => reservation.ApplyInstallment(20_000m))
            .Code.ShouldBe(BanquetHallManagementDomainErrorCodes.PaymentInvalidReservationStatus);
    }

    [Fact]
    public void Completed_Reservation_Should_Not_Block_Scheduling()
    {
        var reservation = CreateReservation(
            ReservationStatus.Completed,
            new TimeSpan(18, 0, 0),
            new TimeSpan(22, 0, 0),
            eventDate: AsOf.Date);

        reservation.BlocksScheduling(AsOf).ShouldBeFalse();
    }

    [Fact]
    public void Archive_Should_Set_Status_To_Archived_For_Pending_Reservation()
    {
        var reservation = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        reservation.Archive();

        reservation.Status.ShouldBe(ReservationStatus.Archived);
    }

    [Fact]
    public void Archive_Should_Allow_Confirmed_And_Cancelled_Reservations()
    {
        var confirmed = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        confirmed.Archive();
        confirmed.Status.ShouldBe(ReservationStatus.Archived);

        var cancelled = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        cancelled.CancelWithReason(CancellationType.Manual);
        cancelled.Archive();
        cancelled.Status.ShouldBe(ReservationStatus.Archived);
    }

    [Fact]
    public void Archive_Should_Reject_Completed_And_Already_Archived_Reservations()
    {
        var completed = CreateReservation(ReservationStatus.FullyPaid, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        completed.TotalPrice = 100_000m;
        completed.PaidAmount = 100_000m;
        completed.CompleteReservation(new DateTime(2026, 6, 10, 23, 0, 0));

        Should.Throw<BusinessException>(() => completed.Archive());

        var archived = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        archived.Archive();

        Should.Throw<BusinessException>(() => archived.Archive());
    }

    [Fact]
    public void Archived_Reservation_Should_Not_Block_Scheduling()
    {
        var archived = CreateReservation(ReservationStatus.Pending, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0), eventDate: AsOf.Date);
        archived.Archive();

        archived.BlocksScheduling(AsOf).ShouldBeFalse();
    }

    private static Reservation CreateReservation(
        ReservationStatus status,
        TimeSpan startTime,
        TimeSpan endTime,
        DateTime? eventDate = null)
    {
        return new Reservation(Guid.NewGuid())
        {
            HallId = HallId,
            CustomerId = Guid.NewGuid(),
            EventDate = eventDate ?? AsOf.Date,
            StartTime = startTime,
            EndTime = endTime,
            GuestsCount = 100,
            TotalPrice = 1000m,
            Status = status,
        };
    }
}
