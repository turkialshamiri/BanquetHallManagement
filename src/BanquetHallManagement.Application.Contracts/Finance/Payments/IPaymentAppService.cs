using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance.Payments;

public interface IPaymentAppService : IApplicationService
{
    Task<PaymentDto> RecordDepositAsync(RecordDepositDto input);

    Task<InstallmentPaymentResultDto> RecordInstallmentAsync(RecordInstallmentDto input);

    Task<ListResultDto<PaymentDto>> GetByReservationAsync(Guid reservationId);
}
