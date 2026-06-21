using Microsoft.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Reservations.Configurations;

public static class ReservationsDbContextModelCreatingExtensions
{
    public static void ConfigureReservations(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ReservationConfiguration());
        builder.ApplyConfiguration(new ReservationServiceConfiguration());
    }
}
