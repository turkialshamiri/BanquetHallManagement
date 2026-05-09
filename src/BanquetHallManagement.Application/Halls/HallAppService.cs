using BanquetHallManagement.Entities;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Linq;
namespace BanquetHallManagement.Halls
{
    public class HallAppService :
        CrudAppService<
            Hall,
            HallDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateHallDto>,
        IHallAppService
    {
        public HallAppService(IRepository<Hall, Guid> repository)
            : base(repository)
        {

        }

        public async Task<List<HallDto>> GetByStatusAsync(HallStatus status)
        {
            var query = await Repository.GetQueryableAsync();

            var halls = await AsyncExecuter.ToListAsync(
                query.Where(x => x.Status == status)
            );

            return ObjectMapper.Map<List<Hall>, List<HallDto>>(halls);
        }
    }
}