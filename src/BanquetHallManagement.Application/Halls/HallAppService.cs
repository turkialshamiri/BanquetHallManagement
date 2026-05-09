using BanquetHallManagement.Entities;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

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
    }
}