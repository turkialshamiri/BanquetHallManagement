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

    [Fact]
    public void Complete_Should_Require_FullyPaid_Status()
    {
        var reservation = CreateReservation(ReservationStatus.Confirmed, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));

        Should.Throw<BusinessException>(() => reservation.Complete())
            .Code.ShouldBe(BanquetHallManagementDomainErrorCodes.ReservationCannotComplete);
    }

    [Fact]
    public void Complete_Should_Move_FullyPaid_Reservation_To_Completed()
    {
        var reservation = CreateReservation(ReservationStatus.FullyPaid, new TimeSpan(18, 0, 0), new TimeSpan(22, 0, 0));
        reservation.TotalPrice = 1000m;
        reservation.PaidAmount = 1000m;

        reservation.Complete();

        reservation.Status.ShouldBe(ReservationStatus.Completed);
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
