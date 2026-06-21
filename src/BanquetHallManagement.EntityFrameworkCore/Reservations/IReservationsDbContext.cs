using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Reservations;

[ConnectionStringName("Default")]
public interface IReservationsDbContext : IEfCoreDbContext
{
    DbSet<Reservation> Reservations { get; }
}
