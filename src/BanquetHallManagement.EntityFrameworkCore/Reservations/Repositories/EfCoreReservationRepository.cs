using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using BanquetHallManagement.ReservationServices;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Guids;

namespace BanquetHallManagement.EntityFrameworkCore.Reservations.Repositories;

public class EfCoreReservationRepository :
    EfCoreRepository<BanquetHallManagementDbContext, Reservation, Guid>,
    IReservationRepository
{
    private readonly IGuidGenerator _guidGenerator;

    public EfCoreReservationRepository(
        IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider,
        IGuidGenerator guidGenerator)
        : base(dbContextProvider)
    {
        _guidGenerator = guidGenerator;
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

    public async Task<List<Reservation>> GetListByHallAndEventDateAsync(
        Guid hallId,
        DateTime eventDate,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await AsyncExecuter.ToListAsync(
            query.Where(reservation =>
                reservation.HallId == hallId &&
                reservation.EventDate.Date == eventDate.Date &&
                (!excludeReservationId.HasValue || reservation.Id != excludeReservationId.Value)),
            cancellationToken);
    }

    public async Task<List<Reservation>> GetPendingByHallAndEventDateAsync(
        Guid hallId,
        DateTime eventDate,
        Guid excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await AsyncExecuter.ToListAsync(
            query.Where(reservation =>
                reservation.Id != excludeReservationId &&
                reservation.HallId == hallId &&
                reservation.EventDate.Date == eventDate.Date &&
                reservation.Status == ReservationStatus.Pending),
            cancellationToken);
    }

    public async Task<List<Reservation>> GetConfirmedWithoutPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await AsyncExecuter.ToListAsync(
            query.Where(reservation =>
                reservation.Status == ReservationStatus.Confirmed &&
                reservation.PaidAmount <= 0m),
            cancellationToken);
    }

    public async Task<bool> IsServiceReferencedAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        return await dbContext.Set<ReservationService>()
            .AnyAsync(link => link.ServiceId == serviceId, cancellationToken);
    }

    public async Task SyncReservationServicesAsync(
        Guid reservationId,
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var linkSet = dbContext.Set<ReservationService>();
        var requestedServiceIds = serviceIds?.ToHashSet() ?? [];

        var existingLinks = await linkSet
            .Where(link => link.ReservationId == reservationId)
            .ToListAsync(cancellationToken);

        foreach (var link in existingLinks.Where(link => !requestedServiceIds.Contains(link.ServiceId)))
        {
            linkSet.Remove(link);
        }

        var existingServiceIds = existingLinks
            .Select(link => link.ServiceId)
            .ToHashSet();

        foreach (var serviceId in requestedServiceIds.Where(id => !existingServiceIds.Contains(id)))
        {
            await linkSet.AddAsync(
                new ReservationService(_guidGenerator.Create())
                {
                    ReservationId = reservationId,
                    ServiceId = serviceId,
                },
                cancellationToken);
        }
    }

    internal static string BuildSchedulingLockResource(Guid hallId, DateTime eventDate)
    {
        return $"BHM_Reservation_{hallId:N}_{eventDate:yyyyMMdd}";
    }
}
