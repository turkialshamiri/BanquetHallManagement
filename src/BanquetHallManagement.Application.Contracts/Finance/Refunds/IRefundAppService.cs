using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance.Refunds;

public interface IRefundAppService : IApplicationService
{
    Task<ListResultDto<PendingRefundDto>> GetPendingAsync(RefundLiabilityGetListInput input);

    Task<RefundLiabilityLookupDto> GetByReservationNumberAsync(string reservationNumber);

    Task<ProcessRefundResultDto> ProcessAsync(Guid reservationId);

    Task<ProcessRefundResultDto> ProcessByReservationNumberAsync(string reservationNumber);
}
