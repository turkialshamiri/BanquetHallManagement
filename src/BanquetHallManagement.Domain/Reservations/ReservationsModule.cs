using BanquetHallManagement;
using Volo.Abp.Modularity;

namespace BanquetHallManagement.Reservations;

/// <summary>
/// Reservations bounded context — domain layer module.
/// </summary>
[DependsOn(typeof(BanquetHallManagementDomainSharedModule))]
public class ReservationsDomainModule : AbpModule
{
}
