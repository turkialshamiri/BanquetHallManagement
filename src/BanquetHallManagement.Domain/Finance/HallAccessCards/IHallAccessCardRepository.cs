using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.HallAccessCards;

public interface IHallAccessCardRepository : IRepository<HallAccessCard, Guid>
{
    Task<HallAccessCard?> FindByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);
}
