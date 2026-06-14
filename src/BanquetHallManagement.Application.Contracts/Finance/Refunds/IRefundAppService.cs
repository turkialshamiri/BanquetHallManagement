using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance.Refunds;

public interface IRefundAppService : IApplicationService
{
    Task<PagedResultDto<PendingRefundDto>> GetPendingAsync(RefundLiabilityGetListInput input);

    Task<RefundDetailsDto> GetDetailsAsync(Guid reservationId);

    Task<RefundLiabilityLookupDto> GetByReservationNumberAsync(string reservationNumber);

    Task<ProcessRefundResultDto> ProcessAsync(Guid reservationId);

    Task<ProcessRefundResultDto> ProcessByReservationNumberAsync(string reservationNumber);
}
