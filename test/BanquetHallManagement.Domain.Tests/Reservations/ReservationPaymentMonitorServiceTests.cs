using System;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using Shouldly;
using Xunit;

namespace BanquetHallManagement.Reservations;

public class ReservationPaymentMonitorServiceTests
{
    private static readonly DateTime Now = new(2026, 6, 12, 17, 30, 0);

    [Fact]
    public void IsEligibleForAutoCancel_Should_Return_True_When_Confirmed_Unpaid_And_Within_Two_Hours()
    {
        var service = new ReservationPaymentMonitorService(null!, null!);
        var reservation = CreateReservation(
            ReservationStatus.Confirmed,
            paidAmount: 0m,
            eventDate: Now.Date,
            startTime: new TimeSpan(19, 0, 0));

        service.IsEligibleForAutoCancel(reservation, Now).ShouldBeTrue();
    }

    [Fact]
    public void IsEligibleForAutoCancel_Should_Return_False_When_Deposit_Recorded()
    {
        var service = new ReservationPaymentMonitorService(null!, null!);
        var reservation = CreateReservation(
            ReservationStatus.Confirmed,
            paidAmount: 30_000m,
            eventDate: Now.Date,
            startTime: new TimeSpan(19, 0, 0));

        service.IsEligibleForAutoCancel(reservation, Now).ShouldBeFalse();
    }

    [Fact]
    public void IsEligibleForAutoCancel_Should_Return_False_When_Fully_Paid()
    {
        var service = new ReservationPaymentMonitorService(null!, null!);
        var reservation = CreateReservation(
            ReservationStatus.FullyPaid,
            paidAmount: 100_000m,
            eventDate: Now.Date,
            startTime: new TimeSpan(19, 0, 0));

        service.IsEligibleForAutoCancel(reservation, Now).ShouldBeFalse();
    }

    [Fact]
    public void IsEligibleForAutoCancel_Should_Return_False_When_More_Than_Two_Hours_Remaining()
    {
        var service = new ReservationPaymentMonitorService(null!, null!);
        var reservation = CreateReservation(
            ReservationStatus.Confirmed,
            paidAmount: 0m,
            eventDate: Now.Date,
            startTime: new TimeSpan(22, 0, 0));

        service.IsEligibleForAutoCancel(reservation, Now).ShouldBeFalse();
    }

    [Fact]
    public void IsEligibleForAutoCancel_Should_Return_False_When_Event_Already_Started()
    {
        var service = new ReservationPaymentMonitorService(null!, null!);
        var reservation = CreateReservation(
            ReservationStatus.Confirmed,
            paidAmount: 0m,
            eventDate: Now.Date,
            startTime: new TimeSpan(16, 0, 0));

        service.IsEligibleForAutoCancel(reservation, Now).ShouldBeFalse();
    }

    private static Reservation CreateReservation(
        ReservationStatus status,
        decimal paidAmount,
        DateTime eventDate,
        TimeSpan startTime)
    {
        return new Reservation(Guid.NewGuid())
        {
            Status = status,
            TotalPrice = 100_000m,
            PaidAmount = paidAmount,
            EventDate = eventDate,
            StartTime = startTime,
            EndTime = startTime.Add(TimeSpan.FromHours(4)),
        };
    }
}
