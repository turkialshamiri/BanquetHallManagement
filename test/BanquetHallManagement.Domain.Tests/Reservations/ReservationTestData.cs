using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using NSubstitute;

namespace BanquetHallManagement.Reservations;

public static class ReservationTestData
{
    private static int _sequence;

    public static Reservation Create(
        Guid hallId,
        Guid customerId,
        DateTime eventDate,
        TimeSpan startTime,
        TimeSpan endTime,
        int guestsCount = 100,
        ReservationStatus status = ReservationStatus.Pending,
        decimal totalPrice = 1000m,
        decimal paidAmount = 0m,
        Guid? id = null)
    {
        var reservation = Reservation.Create(
            id ?? Guid.NewGuid(),
            hallId,
            customerId,
            new TimeSlot(eventDate, startTime, endTime),
            guestsCount);

        reservation.Status = status;
        reservation.TotalPrice = totalPrice;
        reservation.PaidAmount = paidAmount;

        return reservation;
    }

    public static void AssignReservationNumber(Reservation reservation)
    {
        var number = Interlocked.Increment(ref _sequence);
        reservation.AssignReservationNumber($"RES-2026-{number:D5}");
    }

    public static void ConfigureSchedulingQueries(
        IReservationRepository repository,
        IEnumerable<Reservation> reservations)
    {
        var reservationList = reservations.ToList();
        repository.GetListByHallAndEventDateAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateTime>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var hallId = callInfo.ArgAt<Guid>(0);
                var eventDate = callInfo.ArgAt<DateTime>(1);
                var excludeReservationId = callInfo.Arg<Guid?>();

                return Task.FromResult(reservationList
                    .Where(reservation =>
                        reservation.HallId == hallId &&
                        reservation.EventDate.Date == eventDate.Date &&
                        (!excludeReservationId.HasValue || reservation.Id != excludeReservationId.Value))
                    .ToList());
            });

        repository.GetPendingByHallAndEventDateAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateTime>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var hallId = callInfo.ArgAt<Guid>(0);
                var eventDate = callInfo.ArgAt<DateTime>(1);
                var excludeReservationId = callInfo.ArgAt<Guid>(2);

                return Task.FromResult(reservationList
                    .Where(reservation =>
                        reservation.Id != excludeReservationId &&
                        reservation.HallId == hallId &&
                        reservation.EventDate.Date == eventDate.Date &&
                        reservation.Status == ReservationStatus.Pending)
                    .ToList());
            });

        repository.GetConfirmedWithoutPaymentsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(reservationList
                .Where(reservation =>
                    reservation.Status == ReservationStatus.Confirmed &&
                    reservation.PaidAmount <= 0m)
                .ToList()));
    }
}
