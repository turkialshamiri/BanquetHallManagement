using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Services
{
    public class ServiceAppService :
        ApplicationService,
        IServiceAppService
    {
        private readonly IRepository<Service, Guid> _serviceRepository;

        public ServiceAppService(
            IRepository<Service, Guid> serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<ServiceDto> GetAsync(Guid id)
        {
            var service = await _serviceRepository.GetAsync(id);

            return ObjectMapper.Map<Service, ServiceDto>(service);
        }

        public async Task<PagedResultDto<ServiceDto>> GetListAsync(
            PagedAndSortedResultRequestDto input)
        {
            var query = await _serviceRepository.GetQueryableAsync();

            var totalCount = query.Count();

            var services = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<ServiceDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Service>, List<ServiceDto>>(services)
            };
        }

        public async Task<ServiceDto> CreateAsync(
            CreateUpdateServiceDto input)
        {
            // Validation

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException(
                    "اسم الخدمة مطلوب");
            }

            if (input.Price <= 0)
            {
                throw new UserFriendlyException(
                    "سعر الخدمة يجب أن يكون أكبر من صفر");
            }

            var exists = await _serviceRepository.AnyAsync(
                x => x.Name == input.Name);

            if (exists)
            {
                throw new UserFriendlyException(
                    "الخدمة موجودة مسبقاً");
            }

            var service = ObjectMapper.Map<
                CreateUpdateServiceDto,
                Service>(input);

            await _serviceRepository.InsertAsync(service);

            return ObjectMapper.Map<Service, ServiceDto>(service);
        }

        public async Task<ServiceDto> UpdateAsync(
            Guid id,
            CreateUpdateServiceDto input)
        {
            var service = await _serviceRepository.GetAsync(id);

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException(
                    "اسم الخدمة مطلوب");
            }

            if (input.Price <= 0)
            {
                throw new UserFriendlyException(
                    "سعر الخدمة يجب أن يكون أكبر من صفر");
            }

            var exists = await _serviceRepository.AnyAsync(
                x => x.Name == input.Name && x.Id != id);

            if (exists)
            {
                throw new UserFriendlyException(
                    "يوجد خدمة أخرى بنفس الاسم");
            }

            service.Name = input.Name;
            service.Price = input.Price;

            await _serviceRepository.UpdateAsync(service);

            return ObjectMapper.Map<Service, ServiceDto>(service);
        }

        public async Task DeleteAsync(Guid id)
        {
            var exists = await _serviceRepository.AnyAsync(x => x.Id == id);

            if (!exists)
            {
                throw new UserFriendlyException(
                    "الخدمة غير موجودة");
            }

            await _serviceRepository.DeleteAsync(id);
        }
    }
}