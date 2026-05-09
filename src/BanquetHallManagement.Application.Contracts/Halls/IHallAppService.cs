using BanquetHallManagement.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Halls
{
    public interface IHallAppService :
        ICrudAppService<
            HallDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateHallDto>
    {
        Task<List<HallDto>> GetByStatusAsync(HallStatus status);
    }
}