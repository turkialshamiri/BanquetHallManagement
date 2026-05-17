using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Halls
{
    public class HallAppService : ApplicationService, IHallAppService
    {
        private readonly IRepository<Hall, Guid> _hallRepository;

        public HallAppService(IRepository<Hall, Guid> hallRepository)
        {
            _hallRepository = hallRepository;
        }

        public async Task<HallDto> GetAsync(Guid id)
        {
            var hall = await _hallRepository.GetAsync(id);

            return ObjectMapper.Map<Hall, HallDto>(hall);
        }

        public async Task<PagedResultDto<HallDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await _hallRepository.GetQueryableAsync();

            var totalCount = query.Count();

            var halls = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<HallDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Hall>, List<HallDto>>(halls)
            };
        }

        public async Task<HallDto> CreateAsync(CreateUpdateHallDto input)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException("اسم القاعة مطلوب");
            }

            if (input.Capacity <= 0)
            {
                throw new UserFriendlyException("السعة يجب أن تكون أكبر من صفر");
            }

            if (input.PricePerHour <= 0)
            {
                throw new UserFriendlyException("السعر يجب أن يكون أكبر من صفر");
            }

            var hall = ObjectMapper.Map<CreateUpdateHallDto, Hall>(input);

            await _hallRepository.InsertAsync(hall);

            return ObjectMapper.Map<Hall, HallDto>(hall);
        }

        public async Task<HallDto> UpdateAsync(Guid id, CreateUpdateHallDto input)
        {
            var hall = await _hallRepository.GetAsync(id);

            hall.Name = input.Name;
            hall.Description = input.Description;
            hall.Capacity = input.Capacity;
            hall.PricePerHour = input.PricePerHour;
            hall.Location = input.Location;
            hall.Status = input.Status;
            hall.Type = input.Type;

            await _hallRepository.UpdateAsync(hall);

            return ObjectMapper.Map<Hall, HallDto>(hall);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _hallRepository.DeleteAsync(id);
        }

        public async Task<List<HallDto>> GetByStatusAsync(HallStatus status)
        {
            var query = await _hallRepository.GetQueryableAsync();

            var halls = await AsyncExecuter.ToListAsync(
                query.Where(x => x.Status == status)
            );

            return ObjectMapper.Map<List<Hall>, List<HallDto>>(halls);
        }
    }
}