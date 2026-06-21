using BanquetHallManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace BanquetHallManagement.Reservations;

[Authorize(BanquetHallManagementPermissions.Reservations.Default)]
public class ReservationAppService :
    ApplicationService,
    IReservationAppService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly ReservationCreationService _creationService;
    private readonly ReservationSchedulingService _schedulingService;
    private readonly ReservationLifecycleService _lifecycleService;

    public ReservationAppService(
        IReservationRepository reservationRepository,
        IIdentityUserRepository identityUserRepository,
        ReservationCreationService creationService,
        ReservationSchedulingService schedulingService,
        ReservationLifecycleService lifecycleService)
    {
        _reservationRepository = reservationRepository;
        _identityUserRepository = identityUserRepository;
        _creationService = creationService;
        _schedulingService = schedulingService;
        _lifecycleService = lifecycleService;
    }

    public async Task<ReservationDto> GetAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);

        return await MapToDtoAsync(reservation);
    }

    public async Task<PagedResultDto<ReservationDto>> GetListAsync(
        ReservationGetListInput input)
    {
        var query = await _reservationRepository.WithDetailsAsync();
        query = query.WhereActive();

        if (input.HallId.HasValue)
        {
            query = query.Where(reservation => reservation.HallId == input.HallId.Value);
        }

        if (input.CustomerId.HasValue)
        {
            query = query.Where(reservation => reservation.CustomerId == input.CustomerId.Value);
        }

        if (input.EventDateFrom.HasValue)
        {
            var fromDate = input.EventDateFrom.Value.Date;
            query = query.Where(reservation => reservation.EventDate.Date >= fromDate);
        }

        if (input.EventDateTo.HasValue)
        {
            var toDate = input.EventDateTo.Value.Date;
            query = query.Where(reservation => reservation.EventDate.Date <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(input.ReservationNumber))
        {
            var reservationNumber = input.ReservationNumber.Trim();
            query = query.Where(reservation =>
                reservation.ReservationNumber.Contains(reservationNumber));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(reservation => reservation.Status == input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var reservations = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<ReservationDto>
        {
            TotalCount = totalCount,
            Items = reservations.Select(r => MapToDto(r)).ToList()
        };
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Create)]
    public Task<ReservationDto> CreateAsync(CreateUpdateReservationDto input)
    {
        return _schedulingService.ExecuteAsync(async () =>
        {
            var (reservation, serviceIds) = await _creationService.CreateAsync(input);
            return MapToDto(reservation, serviceIds);
        });
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Update)]
    public Task<ReservationDto> UpdateAsync(Guid id, CreateUpdateReservationDto input)
    {
        return _schedulingService.ExecuteAsync(async () =>
        {
            var (reservation, serviceIds) = await _creationService.UpdateAsync(id, input);
            return MapToDto(reservation, serviceIds);
        });
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Delete)]
    public Task DeleteAsync(Guid id)
    {
        return _lifecycleService.DeleteAsync(id);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Delete)]
    public async Task<ReservationDto> ArchiveAsync(Guid id)
    {
        var reservation = await _lifecycleService.ArchiveAsync(id);
        return await MapToDtoAsync(reservation);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Confirm)]
    public Task<ReservationDto> ConfirmAsync(Guid id)
    {
        return _schedulingService.ExecuteAsync(async () =>
        {
            var reservation = await _lifecycleService.ConfirmAsync(id);
            return MapToDto(reservation);
        });
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Cancel)]
    public async Task<ReservationDto> CancelAsync(Guid id)
    {
        var reservation = await _lifecycleService.CancelAsync(id);
        return MapToDto(reservation);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.ConfirmHallEntry)]
    public async Task<ReservationDto> ConfirmHallEntryAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);
        reservation = await _lifecycleService.ConfirmHallEntryAsync(reservation);
        return MapToDto(reservation);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.ConfirmHallEntry)]
    public async Task<ReservationDto> ConfirmHallEntryByReservationNumberAsync(
        ConfirmHallEntryByNumberDto input)
    {
        var reservation = await _lifecycleService.GetByReservationNumberOrThrowAsync(input.ReservationNumber);
        reservation = await _lifecycleService.ConfirmHallEntryAsync(reservation);
        return MapToDto(reservation);
    }

    public async Task<ReservationDto> GetByReservationNumberAsync(string reservationNumber)
    {
        var reservation = await _lifecycleService.GetByReservationNumberOrThrowAsync(reservationNumber);
        return await MapToDtoAsync(reservation);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Complete)]
    [System.Obsolete("Use ConfirmHallEntryAsync instead. Hall entry confirmation is the official completion path.")]
    public async Task<ReservationDto> CompleteAsync(Guid id)
    {
        var reservation = await _lifecycleService.CompleteAsync(id);
        return MapToDto(reservation);
    }

    private async Task<ReservationDto> MapToDtoAsync(
        Reservation reservation,
        IReadOnlyList<Guid> serviceIds = null)
    {
        var dto = MapToDto(reservation, serviceIds);
        dto.CreatedBy = await ResolveUserDisplayNameAsync(reservation.CreatorId);
        dto.LastModifiedBy = await ResolveUserDisplayNameAsync(reservation.LastModifierId);

        return dto;
    }

    private async Task<string?> ResolveUserDisplayNameAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        var user = await _identityUserRepository.FindAsync(userId.Value);
        if (user == null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(user.Name)
            ? user.UserName
            : user.Name;
    }

    private ReservationDto MapToDto(
        Reservation reservation,
        IReadOnlyList<Guid> serviceIds = null)
    {
        var dto = ObjectMapper.Map<Reservation, ReservationDto>(reservation);
        dto.Status = reservation.Status.ToString();
        dto.RemainingAmount = reservation.GetRemainingAmount();
        dto.CancellationType = reservation.CancellationType?.ToString();
        dto.ServiceIds = serviceIds?.ToList()
            ?? reservation.Services?.Select(rs => rs.ServiceId).ToList()
            ?? [];
        dto.CreationTime = reservation.CreationTime;
        dto.LastModificationTime = reservation.LastModificationTime;

        return dto;
    }
}
