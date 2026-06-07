using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Halls;
using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Reservations;

public class ReservationAppService :
    ApplicationService,
    IReservationAppService
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;
    private readonly IRepository<ReservationService, Guid> _reservationServiceRepository;
    private readonly HallAvailabilityManager _hallAvailabilityManager;

    public ReservationAppService(
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Service, Guid> serviceRepository,
        IRepository<ReservationService, Guid> reservationServiceRepository,
        HallAvailabilityManager hallAvailabilityManager)
    {
        _reservationRepository = reservationRepository;
        _hallRepository = hallRepository;
        _serviceRepository = serviceRepository;
        _reservationServiceRepository = reservationServiceRepository;
        _hallAvailabilityManager = hallAvailabilityManager;
    }

    public async Task<ReservationDto> GetAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        return MapToDto(reservation);
    }

    public async Task<PagedResultDto<ReservationDto>> GetListAsync(
        PagedAndSortedResultRequestDto input)
    {
        var query = await _reservationRepository.GetQueryableAsync();

        var totalCount = query.Count();

        var reservations = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<ReservationDto>
        {
            TotalCount = totalCount,
            Items = reservations.Select(MapToDto).ToList()
        };
    }

    public async Task<ReservationDto> CreateAsync(
        CreateUpdateReservationDto input)
    {
        ValidateReservationTimes(input);

        if (input.GuestsCount <= 0)
        {
            throw new UserFriendlyException(
                "عدد الضيوف يجب أن يكون أكبر من صفر");
        }

        var hall = await _hallRepository.GetAsync(input.HallId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        if (input.GuestsCount > hall.Capacity)
        {
            throw new UserFriendlyException(
                "عدد الضيوف يتجاوز سعة القاعة");
        }

        await EnsureNoSchedulingConflictAsync(
            input.HallId,
            input.EventDate,
            input.StartTime,
            input.EndTime);

        var reservationHours =
            (input.EndTime - input.StartTime).TotalHours;

        decimal totalPrice =
            (decimal)reservationHours * hall.PricePerHour;

        var reservation = new Reservation
        {
            HallId = input.HallId,
            CustomerId = input.CustomerId,
            EventDate = input.EventDate,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            GuestsCount = input.GuestsCount,
            Status = ReservationStatus.Pending,
            TotalPrice = 0
        };

        await _reservationRepository.InsertAsync(
            reservation,
            autoSave: true);

        if (input.ServiceIds != null && input.ServiceIds.Any())
        {
            var services = await _serviceRepository.GetListAsync(
                s => input.ServiceIds.Contains(s.Id));

            totalPrice += services.Sum(s => s.Price);

            foreach (var service in services)
            {
                await _reservationServiceRepository.InsertAsync(
                    new ReservationService
                    {
                        ReservationId = reservation.Id,
                        ServiceId = service.Id
                    });
            }
        }

        reservation.TotalPrice = totalPrice;

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    public async Task<ReservationDto> UpdateAsync(
        Guid id,
        CreateUpdateReservationDto input)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        if (!reservation.CanBeUpdated())
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotUpdate);
        }

        ValidateReservationTimes(input);

        if (input.GuestsCount <= 0)
        {
            throw new UserFriendlyException(
                "عدد الضيوف يجب أن يكون أكبر من صفر");
        }

        var hall = await _hallRepository.GetAsync(input.HallId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        if (input.GuestsCount > hall.Capacity)
        {
            throw new UserFriendlyException(
                "عدد الضيوف يتجاوز سعة القاعة");
        }

        await EnsureNoSchedulingConflictAsync(
            input.HallId,
            input.EventDate,
            input.StartTime,
            input.EndTime,
            reservation.Id);

        reservation.EventDate = input.EventDate;
        reservation.StartTime = input.StartTime;
        reservation.EndTime = input.EndTime;
        reservation.GuestsCount = input.GuestsCount;

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    public async Task DeleteAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        if (!reservation.CanBeDeleted())
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotDelete);
        }

        await _reservationRepository.DeleteAsync(id);
    }

    public async Task<ReservationDto> ConfirmAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        await EnsureNoSchedulingConflictAsync(
            reservation.HallId,
            reservation.EventDate,
            reservation.StartTime,
            reservation.EndTime,
            reservation.Id);

        reservation.Confirm();

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    public async Task<ReservationDto> CancelAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        reservation.Cancel();

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    public async Task<ReservationDto> CompleteAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        reservation.Complete();

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    private static void ValidateReservationTimes(CreateUpdateReservationDto input)
    {
        if (input.StartTime >= input.EndTime)
        {
            throw new UserFriendlyException(
                "وقت البداية يجب أن يكون أقل من وقت النهاية");
        }
    }

    private async Task EnsureNoSchedulingConflictAsync(
        Guid hallId,
        DateTime eventDate,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeReservationId = null)
    {
        var now = Clock.Now;
        var query = await _reservationRepository.GetQueryableAsync();

        var candidate = new Reservation
        {
            HallId = hallId,
            EventDate = eventDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = ReservationStatus.Pending
        };

        var existingReservations = await AsyncExecuter.ToListAsync(
            query.Where(r =>
                r.HallId == hallId &&
                r.EventDate.Date == eventDate.Date &&
                (!excludeReservationId.HasValue || r.Id != excludeReservationId.Value)));

        var hasConflict = existingReservations.Any(existing =>
            candidate.HasSchedulingConflictWith(existing, now));

        if (hasConflict)
        {
            throw new UserFriendlyException(
                "هذه القاعة محجوزة بالفعل في هذا الوقت");
        }
    }

    private ReservationDto MapToDto(Reservation reservation)
    {
        var dto = ObjectMapper.Map<Reservation, ReservationDto>(reservation);
        dto.Status = reservation.Status.ToString();
        return dto;
    }
}
