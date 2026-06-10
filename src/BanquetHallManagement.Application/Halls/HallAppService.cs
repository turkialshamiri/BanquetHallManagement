using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Halls
{
    [Authorize(BanquetHallManagementPermissions.Halls.Default)]
    public class HallAppService : ApplicationService, IHallAppService
    {
        private readonly IRepository<Hall, Guid> _hallRepository;
        private readonly IRepository<Reservation, Guid> _reservationRepository;
        private readonly HallAvailabilityManager _hallAvailabilityManager;

        public HallAppService(
            IRepository<Hall, Guid> hallRepository,
            IRepository<Reservation, Guid> reservationRepository,
            HallAvailabilityManager hallAvailabilityManager)
        {
            _hallRepository = hallRepository;
            _reservationRepository = reservationRepository;
            _hallAvailabilityManager = hallAvailabilityManager;
        }

        public async Task<HallDto> GetAsync(Guid id)
        {
            var hall = await _hallRepository.GetAsync(id);
            var bookedHallIds = await GetHallIdsWithActiveConfirmedReservationsAsync();

            return MapToHallDto(hall, bookedHallIds);
        }

        public async Task<PagedResultDto<HallDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await _hallRepository.GetQueryableAsync();
            var bookedHallIds = await GetHallIdsWithActiveConfirmedReservationsAsync();

            var totalCount = await AsyncExecuter.CountAsync(query);

            var halls = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(x => x.Name)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount));

            return new PagedResultDto<HallDto>
            {
                TotalCount = totalCount,
                Items = halls
                    .Select(hall => MapToHallDto(hall, bookedHallIds))
                    .ToList()
            };
        }

        [Authorize(BanquetHallManagementPermissions.Halls.Create)]
        public async Task<HallDto> CreateAsync(CreateUpdateHallDto input)
        {
            ValidateInput(input);

            _hallAvailabilityManager.EnsureOperationalStatus(input.Status);

            var hall = ObjectMapper.Map<CreateUpdateHallDto, Hall>(input);
            hall.Status = _hallAvailabilityManager.GetOperationalStatus(hall);

            await _hallRepository.InsertAsync(hall);

            return MapToHallDto(hall, new HashSet<Guid>());
        }

        [Authorize(BanquetHallManagementPermissions.Halls.Update)]
        public async Task<HallDto> UpdateAsync(Guid id, CreateUpdateHallDto input)
        {
            ValidateInput(input);

            _hallAvailabilityManager.EnsureOperationalStatus(input.Status);

            var hall = await _hallRepository.GetAsync(id);

            hall.Name = input.Name;
            hall.Description = input.Description;
            hall.Capacity = input.Capacity;
            hall.PricePerHour = input.PricePerHour;
            hall.Location = input.Location;
            hall.Status = input.Status;
            hall.Type = input.Type;

            await _hallRepository.UpdateAsync(hall);

            var bookedHallIds = await GetHallIdsWithActiveConfirmedReservationsAsync();

            return MapToHallDto(hall, bookedHallIds);
        }

        [Authorize(BanquetHallManagementPermissions.Halls.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _hallRepository.GetAsync(id);

            var hasReservations = await _reservationRepository.AnyAsync(r => r.HallId == id);
            if (hasReservations)
            {
                throw new BusinessException(
                    BanquetHallManagementDomainErrorCodes.HallCannotDeleteHasReservations);
            }

            await _hallRepository.DeleteAsync(id);
        }

        public async Task<List<HallDto>> GetByStatusAsync(HallStatus status)
        {
            var query = await _hallRepository.GetQueryableAsync();
            var bookedHallIds = await GetHallIdsWithActiveConfirmedReservationsAsync();
            var halls = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Name));

            return halls
                .Select(hall => MapToHallDto(hall, bookedHallIds))
                .Where(dto => dto.Status == status)
                .ToList();
        }

        private void ValidateInput(CreateUpdateHallDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException(L["Validation:HallNameRequired"]);
            }

            if (input.Capacity <= 0)
            {
                throw new UserFriendlyException(L["Validation:CapacityMustBePositive"]);
            }

            if (input.PricePerHour <= 0)
            {
                throw new UserFriendlyException(L["Validation:PriceMustBePositive"]);
            }
        }

        private async Task<HashSet<Guid>> GetHallIdsWithActiveConfirmedReservationsAsync()
        {
            var now = Clock.Now;
            var today = now.Date;
            var currentTime = now.TimeOfDay;

            var query = await _reservationRepository.GetQueryableAsync();

            var hallIds = await AsyncExecuter.ToListAsync(
                query
                    .Where(r => r.Status == ReservationStatus.Confirmed)
                    .Where(r =>
                        r.EventDate.Date > today ||
                        (r.EventDate.Date == today && r.EndTime > currentTime))
                    .Select(r => r.HallId)
                    .Distinct());

            return hallIds.ToHashSet();
        }

        private HallDto MapToHallDto(Hall hall, HashSet<Guid> bookedHallIds)
        {
            var dto = ObjectMapper.Map<Hall, HallDto>(hall);
            dto.OperationalStatus = _hallAvailabilityManager.GetOperationalStatus(hall);
            dto.Status = _hallAvailabilityManager.ResolveEffectiveStatus(
                hall,
                bookedHallIds.Contains(hall.Id));

            return dto;
        }
    }
}
