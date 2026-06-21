using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Reservations;

/// <summary>
/// Read-only cross-module contract for reservation lookups.
/// Implementation will be added during module extraction.
/// </summary>
public interface IReservationLookupAppService : IApplicationService
{
    Task<ReservationLookupDto> GetAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
}
