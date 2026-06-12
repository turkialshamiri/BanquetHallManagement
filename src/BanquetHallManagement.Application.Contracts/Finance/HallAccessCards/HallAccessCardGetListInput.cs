using Volo.Abp.Application.Dtos;

namespace BanquetHallManagement.Finance.HallAccessCards;

public class HallAccessCardGetListInput : PagedAndSortedResultRequestDto
{
    public string? ReservationNumber { get; set; }
}
