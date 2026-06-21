using BanquetHallManagement.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Halls
{
    public interface IHallAppService : IApplicationService
    {
        Task<HallDto> GetAsync(Guid id);

        Task<PagedResultDto<HallDto>> GetListAsync(PagedAndSortedResultRequestDto input);

        Task<HallDto> CreateAsync(CreateUpdateHallDto input);

        Task<HallDto> UpdateAsync(Guid id, CreateUpdateHallDto input);

        Task DeleteAsync(Guid id);

        Task<List<HallDto>> GetByStatusAsync(HallStatus status);
    }
}