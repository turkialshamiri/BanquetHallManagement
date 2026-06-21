using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Services
{
    [Authorize(BanquetHallManagementPermissions.Services.Default)]
    public class ServiceAppService :
        ApplicationService,
        IServiceAppService
    {
        private readonly IRepository<Service, Guid> _serviceRepository;
        private readonly IReservationRepository _reservationRepository;

        public ServiceAppService(
            IRepository<Service, Guid> serviceRepository,
            IReservationRepository reservationRepository)
        {
            _serviceRepository = serviceRepository;
            _reservationRepository = reservationRepository;
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

            var services = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(service => service.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount));

            return new PagedResultDto<ServiceDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Service>, List<ServiceDto>>(services)
            };
        }

        [Authorize(BanquetHallManagementPermissions.Services.Create)]
        public async Task<ServiceDto> CreateAsync(
            CreateUpdateServiceDto input)
        {
            // Validation

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException(L["Validation:ServiceNameRequired"]);
            }

            if (input.Price <= 0)
            {
                throw new UserFriendlyException(L["Validation:ServicePriceMustBePositive"]);
            }

            var exists = await _serviceRepository.AnyAsync(
                x => x.Name == input.Name);

            if (exists)
            {
                throw new UserFriendlyException(L["Validation:ServiceNameDuplicate"]);
            }

            var service = ObjectMapper.Map<
                CreateUpdateServiceDto,
                Service>(input);

            await _serviceRepository.InsertAsync(service);

            return ObjectMapper.Map<Service, ServiceDto>(service);
        }

        [Authorize(BanquetHallManagementPermissions.Services.Update)]
        public async Task<ServiceDto> UpdateAsync(
            Guid id,
            CreateUpdateServiceDto input)
        {
            var service = await _serviceRepository.GetAsync(id);

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException(L["Validation:ServiceNameRequired"]);
            }

            if (input.Price <= 0)
            {
                throw new UserFriendlyException(L["Validation:ServicePriceMustBePositive"]);
            }

            var exists = await _serviceRepository.AnyAsync(
                x => x.Name == input.Name && x.Id != id);

            if (exists)
            {
                throw new UserFriendlyException(L["Validation:ServiceNameDuplicateOther"]);
            }

            service.Name = input.Name;
            service.Price = input.Price;

            await _serviceRepository.UpdateAsync(service);

            return ObjectMapper.Map<Service, ServiceDto>(service);
        }

        [Authorize(BanquetHallManagementPermissions.Services.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _serviceRepository.GetAsync(id);

            var isUsedInReservations = await _reservationRepository.IsServiceReferencedAsync(id);

            if (isUsedInReservations)
            {
                throw new BusinessException(
                    BanquetHallManagementDomainErrorCodes.ServiceCannotDeleteUsedByReservations);
            }

            await _serviceRepository.DeleteAsync(id);
        }
    }
}