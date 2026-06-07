using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Reports;

public interface IReportsAppService : IApplicationService
{
    Task<ReportsResultDto> GetAsync(GetReportsInput input);
}
