using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance;

/// <summary>
/// Read-only cross-module contract for finance lookups.
/// Implementation will be added during module extraction.
/// </summary>
public interface IFinanceLookupAppService : IApplicationService
{
    Task<FinanceLookupDto> GetAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
}
