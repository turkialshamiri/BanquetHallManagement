using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Services
{
    public interface IServiceAppService : IApplicationService
    {
        Task<ServiceDto> GetAsync(Guid id);

        Task<PagedResultDto<ServiceDto>> GetListAsync(
            PagedAndSortedResultRequestDto input);

        Task<ServiceDto> CreateAsync(CreateUpdateServiceDto input);

        Task<ServiceDto> UpdateAsync(
            Guid id,
            CreateUpdateServiceDto input);

        Task DeleteAsync(Guid id);
    }
}