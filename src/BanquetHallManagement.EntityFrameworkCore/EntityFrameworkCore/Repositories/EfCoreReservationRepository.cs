using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Repositories;

public class EfCoreReservationRepository :
    EfCoreRepository<BanquetHallManagementDbContext, Reservation, Guid>,
    IReservationRepository
{
    public EfCoreReservationRepository(IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Reservation>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).Include(x => x.Services);
    }

    public async Task AcquireExclusiveSchedulingLockAsync(
        Guid hallId,
        DateTime eventDate,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var resource = BuildSchedulingLockResource(hallId, eventDate);

        await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
DECLARE @rc INT;
EXEC @rc = sp_getapplock
    @Resource = {resource},
    @LockMode = N'Exclusive',
    @LockOwner = N'Transaction',
    @LockTimeout = 15000;
IF @rc < 0
    THROW 55000, 'Failed to acquire reservation scheduling lock.', 1;", cancellationToken);
    }

    public async Task<Reservation?> FindByReservationNumberAsync(
        string reservationNumber,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(reservation => reservation.ReservationNumber == reservationNumber),
            cancellationToken);
    }

    internal static string BuildSchedulingLockResource(Guid hallId, DateTime eventDate)
    {
        return $"BHM_Reservation_{hallId:N}_{eventDate:yyyyMMdd}";
    }
}
