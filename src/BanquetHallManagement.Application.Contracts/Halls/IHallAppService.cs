using System;
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

    }
}