using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Catalog;

/// <summary>
/// Read-only cross-module contract for catalog master-data lookups.
/// Implementation will be added during module extraction.
/// </summary>
public interface ICatalogLookupAppService : IApplicationService
{
    Task<HallLookupDto> GetHallAsync(Guid id);

    Task<bool> HallExistsAsync(Guid id);

    Task<CustomerLookupDto> GetCustomerAsync(Guid id);

    Task<bool> CustomerExistsAsync(Guid id);

    Task<ServiceLookupDto> GetServiceAsync(Guid id);

    Task<bool> ServiceExistsAsync(Guid id);
}
