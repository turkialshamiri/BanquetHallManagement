using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Services
{
    public interface IServiceAppService :
        ICrudAppService<
            ServiceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateServiceDto>
    {
    }
}