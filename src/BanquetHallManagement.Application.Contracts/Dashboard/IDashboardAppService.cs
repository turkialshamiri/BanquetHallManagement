using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
