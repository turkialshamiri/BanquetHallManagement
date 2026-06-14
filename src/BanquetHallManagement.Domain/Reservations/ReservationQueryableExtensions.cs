using System.Linq;
using BanquetHallManagement.Enums;

namespace BanquetHallManagement.Reservations;

public static class ReservationQueryableExtensions
{
    public static IQueryable<Reservation> WhereActive(this IQueryable<Reservation> query)
    {
        return query.Where(reservation => reservation.Status != ReservationStatus.Archived);
    }
}
