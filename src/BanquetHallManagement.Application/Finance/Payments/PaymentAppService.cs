using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Finance.Payments;

[Authorize(BanquetHallManagementPermissions.Finance.PaymentsView)]
public class PaymentAppService : BanquetHallManagementAppService, IPaymentAppService
{
    private const int SchedulingMaxAttempts = 3;

    private readonly PaymentManager _paymentManager;
    private readonly IRepository<Payment, Guid> _paymentRepository;

    public PaymentAppService(
        PaymentManager paymentManager,
        IRepository<Payment, Guid> paymentRepository)
    {
        _paymentManager = paymentManager;
        _paymentRepository = paymentRepository;
    }

    [Authorize(BanquetHallManagementPermissions.Finance.PaymentsCreate)]
    [Authorize(BanquetHallManagementPermissions.Reservations.RecordPayment)]
    public Task<PaymentDto> RecordDepositAsync(RecordDepositDto input)
    {
        return ExecuteSchedulingOperationAsync(async () =>
        {
            var payment = await _paymentManager.RecordDepositAsync(input.ReservationId, input.Amount);
            await CurrentUnitOfWork.SaveChangesAsync();
            return MapToDto(payment);
        });
    }

    public async Task<ListResultDto<PaymentDto>> GetByReservationAsync(Guid reservationId)
    {
        var query = await _paymentRepository.GetQueryableAsync();

        var payments = await AsyncExecuter.ToListAsync(
            query
                .Where(p => p.ReservationId == reservationId)
                .OrderByDescending(p => p.PaymentDate));

        return new ListResultDto<PaymentDto>(payments.Select(MapToDto).ToList());
    }

    private async Task<T> ExecuteSchedulingOperationAsync<T>(Func<Task<T>> operation)
    {
        for (var attempt = 1; attempt <= SchedulingMaxAttempts; attempt++)
        {
            using var unitOfWork = UnitOfWorkManager.Begin(
                new AbpUnitOfWorkOptions
                {
                    IsTransactional = true,
                    IsolationLevel = IsolationLevel.Serializable
                },
                requiresNew: true);

            try
            {
                var result = await operation();
                await unitOfWork.CompleteAsync();
                return result;
            }
            catch (Exception exception)
                when (ReservationConcurrencyHelper.IsTransientSchedulingFailure(exception) &&
                      attempt < SchedulingMaxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt));
            }
        }

        throw new UserFriendlyException(L["Validation:SchedulingConcurrencyFailed"]);
    }

    private PaymentDto MapToDto(Payment payment)
    {
        var dto = ObjectMapper.Map<Payment, PaymentDto>(payment);
        dto.PaymentType = payment.PaymentType.ToString();
        return dto;
    }
}
