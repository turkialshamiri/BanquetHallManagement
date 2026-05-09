using BanquetHallManagement.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Linq;

namespace BanquetHallManagement.Services
{
    public class ServiceAppService :
        CrudAppService<
            Service,
            ServiceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateServiceDto>,
        IServiceAppService
    {
        public ServiceAppService(IRepository<Service, Guid> repository)
            : base(repository)
        {
        }
    }
}