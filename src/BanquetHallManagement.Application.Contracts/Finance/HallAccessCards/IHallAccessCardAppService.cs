using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance.HallAccessCards;

public interface IHallAccessCardAppService : IApplicationService
{
    Task<PagedResultDto<HallAccessCardListItemDto>> GetListAsync(HallAccessCardGetListInput input);

    Task<HallAccessCardDto> GetAsync(Guid id);

    Task<HallAccessCardDto> GetByReservationAsync(Guid reservationId);

    Task<HallAccessCardDto> GetByReservationNumberAsync(string reservationNumber);

    Task<HallAccessCardEntryPreviewDto> GetEntryPreviewByReservationNumberAsync(
        string reservationNumber);

    Task<HallAccessCardEntryPreviewDto> GetEntryPreviewBySearchAsync(string search);

    Task<HallAccessCardPrintDataDto> GetPrintDataAsync(Guid id);
}
