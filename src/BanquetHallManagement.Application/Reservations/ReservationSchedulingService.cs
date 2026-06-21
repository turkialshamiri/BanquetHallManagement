using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Reservations;

public class ReservationSchedulingService : ITransientDependency
{
    private const int SchedulingMaxAttempts = 3;

    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IStringLocalizer<BanquetHallManagement.Localization.BanquetHallManagementResource> _localizer;

    public ReservationSchedulingService(
        IUnitOfWorkManager unitOfWorkManager,
        IStringLocalizer<BanquetHallManagement.Localization.BanquetHallManagementResource> localizer)
    {
        _unitOfWorkManager = unitOfWorkManager;
        _localizer = localizer;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        for (var attempt = 1; attempt <= SchedulingMaxAttempts; attempt++)
        {
            using var unitOfWork = _unitOfWorkManager.Begin(
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

        throw new UserFriendlyException(_localizer["Validation:SchedulingConcurrencyFailed"]);
    }
}
