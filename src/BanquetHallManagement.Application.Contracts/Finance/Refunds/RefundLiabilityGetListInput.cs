using Volo.Abp.Application.Dtos;

namespace BanquetHallManagement.Finance.Refunds;

public class RefundLiabilityGetListInput : PagedResultRequestDto
{
    public string? ReservationNumber { get; set; }

    public string? Filter { get; set; }
}
