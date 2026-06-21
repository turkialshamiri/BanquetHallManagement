using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Finance.HallAccessCards;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Repositories;

public class EfCoreHallAccessCardRepository :
    EfCoreRepository<BanquetHallManagementDbContext, HallAccessCard, Guid>,
    IHallAccessCardRepository
{
    public EfCoreHallAccessCardRepository(IDbContextProvider<BanquetHallManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<HallAccessCard?> FindByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var trackedCard = dbContext.ChangeTracker
            .Entries<HallAccessCard>()
            .Select(entry => entry.Entity)
            .FirstOrDefault(card => card.ReservationId == reservationId);

        if (trackedCard != null)
        {
            return trackedCard;
        }

        var query = await GetQueryableAsync();

        return await query.FirstOrDefaultAsync(
            card => card.ReservationId == reservationId,
            cancellationToken);
    }

    public async Task<HallAccessCard?> FindByCardNumberAsync(
        string cardNumber,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var trackedCard = dbContext.ChangeTracker
            .Entries<HallAccessCard>()
            .Select(entry => entry.Entity)
            .FirstOrDefault(card => card.CardNumber == cardNumber);

        if (trackedCard != null)
        {
            return trackedCard;
        }

        var query = await GetQueryableAsync();

        return await query.FirstOrDefaultAsync(
            card => card.CardNumber == cardNumber,
            cancellationToken);
    }
}
