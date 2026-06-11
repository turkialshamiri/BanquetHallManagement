using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance.HallAccessCards;

public interface IHallAccessCardAppService : IApplicationService
{
    Task<HallAccessCardDto> GetAsync(Guid id);

    Task<HallAccessCardDto> GetByReservationAsync(Guid reservationId);

    Task<HallAccessCardPrintDataDto> GetPrintDataAsync(Guid id);
}
