using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Reservations;

[Authorize(BanquetHallManagementPermissions.Reservations.Default)]
public class ReservationAppService :
    ApplicationService,
    IReservationAppService
{
    private const int SchedulingMaxAttempts = 3;

    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;
    private readonly IRepository<ReservationService, Guid> _reservationServiceRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly HallAvailabilityManager _hallAvailabilityManager;
    private readonly ReservationSchedulingManager _reservationSchedulingManager;

    public ReservationAppService(
        IReservationRepository reservationRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Service, Guid> serviceRepository,
        IRepository<ReservationService, Guid> reservationServiceRepository,
        IRepository<Customer, Guid> customerRepository,
        HallAvailabilityManager hallAvailabilityManager,
        ReservationSchedulingManager reservationSchedulingManager)
    {
        _reservationRepository = reservationRepository;
        _hallRepository = hallRepository;
        _serviceRepository = serviceRepository;
        _reservationServiceRepository = reservationServiceRepository;
        _customerRepository = customerRepository;
        _hallAvailabilityManager = hallAvailabilityManager;
        _reservationSchedulingManager = reservationSchedulingManager;
    }

    public async Task<ReservationDto> GetAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);

        return MapToDto(reservation);
    }

    public async Task<PagedResultDto<ReservationDto>> GetListAsync(
        PagedAndSortedResultRequestDto input)
    {
        var query = await _reservationRepository.WithDetailsAsync();

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
        return ExecuteSchedulingOperationAsync(() => CreateAsyncCore(input));
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Update)]
    public Task<ReservationDto> UpdateAsync(Guid id, CreateUpdateReservationDto input)
    {
        return ExecuteSchedulingOperationAsync(() => UpdateAsyncCore(id, input));
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        if (!reservation.CanBeDeleted())
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationCannotDelete);
        }

        reservation.MarkForDeletion();

        await _reservationRepository.DeleteAsync(reservation);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Confirm)]
    public Task<ReservationDto> ConfirmAsync(Guid id)
    {
        return ExecuteSchedulingOperationAsync(() => ConfirmAsyncCore(id));
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Cancel)]
    public async Task<ReservationDto> CancelAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);

        reservation.Cancel();

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.Complete)]
    public async Task<ReservationDto> CompleteAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);

        reservation.Complete();

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    private async Task<ReservationDto> CreateAsyncCore(CreateUpdateReservationDto input)
    {
        ValidateReservationTimes(input);

        if (input.GuestsCount <= 0)
        {
            throw new UserFriendlyException(
                "عدد الضيوف يجب أن يكون أكبر من صفر");
        }

        var hall = await _hallRepository.GetAsync(input.HallId);
        await _customerRepository.GetAsync(input.CustomerId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        if (input.GuestsCount > hall.Capacity)
        {
            throw new UserFriendlyException(
                "عدد الضيوف يتجاوز سعة القاعة");
        }

        await _reservationSchedulingManager.EnsureNoSchedulingConflictAsync(
            input.HallId,
            input.EventDate,
            input.StartTime,
            input.EndTime);

        var reservation = new Reservation(GuidGenerator.Create())
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

        await _reservationRepository.InsertAsync(reservation, autoSave: false);

        var services = await LoadRequestedServicesAsync(input.ServiceIds);
        var totalPrice = Reservation.CalculateTotalPrice(
            hall.PricePerHour,
            input.StartTime,
            input.EndTime,
            services.Select(s => s.Price));

        await InsertReservationServicesAsync(reservation.Id, services);

        reservation.FinalizeCreation(totalPrice);

        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(reservation, services.Select(s => s.Id).ToList());
    }

    private async Task<ReservationDto> UpdateAsyncCore(
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

        await _customerRepository.GetAsync(input.CustomerId);

        var hall = await _hallRepository.GetAsync(input.HallId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        if (input.GuestsCount > hall.Capacity)
        {
            throw new UserFriendlyException(
                "عدد الضيوف يتجاوز سعة القاعة");
        }

        await _reservationSchedulingManager.EnsureNoSchedulingConflictAsync(
            input.HallId,
            input.EventDate,
            input.StartTime,
            input.EndTime,
            reservation.Id);

        var services = await LoadRequestedServicesAsync(input.ServiceIds);
        var totalPrice = Reservation.CalculateTotalPrice(
            hall.PricePerHour,
            input.StartTime,
            input.EndTime,
            services.Select(s => s.Price));

        await SyncReservationServicesAsync(reservation.Id, services);

        reservation.ApplyUpdate(
            input.HallId,
            input.CustomerId,
            input.EventDate,
            input.StartTime,
            input.EndTime,
            input.GuestsCount,
            totalPrice);

        await _reservationRepository.UpdateAsync(reservation, autoSave: false);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(reservation, services.Select(s => s.Id).ToList());
    }

    private async Task<ReservationDto> ConfirmAsyncCore(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id, includeDetails: true);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        await _reservationSchedulingManager.EnsureNoSchedulingConflictAsync(
            reservation.HallId,
            reservation.EventDate,
            reservation.StartTime,
            reservation.EndTime,
            reservation.Id);

        reservation.Confirm();

        await _reservationRepository.UpdateAsync(reservation);

        return MapToDto(reservation);
    }

    private async Task<List<Service>> LoadRequestedServicesAsync(List<Guid> serviceIds)
    {
        if (serviceIds == null || serviceIds.Count == 0)
        {
            return [];
        }

        return await _serviceRepository.GetListAsync(s => serviceIds.Contains(s.Id));
    }

    private async Task InsertReservationServicesAsync(
        Guid reservationId,
        IReadOnlyList<Service> services)
    {
        foreach (var service in services)
        {
            await _reservationServiceRepository.InsertAsync(
                new ReservationService(GuidGenerator.Create())
                {
                    ReservationId = reservationId,
                    ServiceId = service.Id
                },
                autoSave: false);
        }
    }

    private async Task SyncReservationServicesAsync(
        Guid reservationId,
        IReadOnlyList<Service> requestedServices)
    {
        var existingLinks = await _reservationServiceRepository.GetListAsync(
            rs => rs.ReservationId == reservationId);

        var requestedServiceIds = requestedServices
            .Select(s => s.Id)
            .ToHashSet();

        foreach (var link in existingLinks.Where(l => !requestedServiceIds.Contains(l.ServiceId)))
        {
            await _reservationServiceRepository.DeleteAsync(link, autoSave: false);
        }

        var existingServiceIds = existingLinks
            .Select(l => l.ServiceId)
            .ToHashSet();

        foreach (var service in requestedServices.Where(s => !existingServiceIds.Contains(s.Id)))
        {
            await _reservationServiceRepository.InsertAsync(
                new ReservationService(GuidGenerator.Create())
                {
                    ReservationId = reservationId,
                    ServiceId = service.Id
                },
                autoSave: false);
        }
    }

    private async Task<T> ExecuteSchedulingOperationAsync<T>(Func<Task<T>> operation)
    {
        for (var attempt = 1; attempt <= SchedulingMaxAttempts; attempt++)
        {
            using var unitOfWork = UnitOfWorkManager.Begin(
                new AbpUnitOfWorkOptions
                {
                    IsTransactional = true,
                    IsolationLevel = IsolationLevel.Serializable
                },
                requiresNew: true);

            try
            {
                var result = await operation();
                await unitOfWork.CompleteAsync();
                return result;
            }
            catch (Exception exception)
                when (ReservationConcurrencyHelper.IsTransientSchedulingFailure(exception) &&
                      attempt < SchedulingMaxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt));
            }
        }

        throw new UserFriendlyException(
            "تعذر إتمام العملية بسبب تعارض حجز متزامن. يرجى المحاولة مرة أخرى.");
    }

    private static void ValidateReservationTimes(CreateUpdateReservationDto input)
    {
        if (input.StartTime >= input.EndTime)
        {
            throw new UserFriendlyException(
                "وقت البداية يجب أن يكون أقل من وقت النهاية");
        }
    }

    private ReservationDto MapToDto(
        Reservation reservation,
        IReadOnlyList<Guid> serviceIds = null)
    {
        var dto = ObjectMapper.Map<Reservation, ReservationDto>(reservation);
        dto.Status = reservation.Status.ToString();
        dto.ServiceIds = serviceIds?.ToList()
            ?? reservation.Services?.Select(rs => rs.ServiceId).ToList()
            ?? [];

        return dto;
    }
}
