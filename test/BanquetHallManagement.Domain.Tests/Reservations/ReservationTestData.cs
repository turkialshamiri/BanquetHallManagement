using System.Threading;

namespace BanquetHallManagement.Reservations;

public static class ReservationTestData
{
    private static int _sequence;

    public static void AssignReservationNumber(Reservation reservation)
    {
        var number = Interlocked.Increment(ref _sequence);
        reservation.AssignReservationNumber($"RES-2026-{number:D5}");
    }
}
