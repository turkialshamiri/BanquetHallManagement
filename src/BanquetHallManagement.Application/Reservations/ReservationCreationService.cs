using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Halls;
using Microsoft.Extensions.Localization;
using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Services;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Reservations;

public class ReservationCreationService : ITransientDependency
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;
    private readonly IRepository<ReservationService, Guid> _reservationServiceRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly HallAvailabilityManager _hallAvailabilityManager;
    private readonly ReservationSchedulingManager _reservationSchedulingManager;
    private readonly IReservationNumberGenerator _reservationNumberGenerator;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly Volo.Abp.Guids.IGuidGenerator _guidGenerator;
    private readonly IStringLocalizer<BanquetHallManagement.Localization.BanquetHallManagementResource> _localizer;

    public ReservationCreationService(
        IReservationRepository reservationRepository,
        IRepository<Hall, Guid> hallRepository,
        IRepository<Service, Guid> serviceRepository,
        IRepository<ReservationService, Guid> reservationServiceRepository,
        IRepository<Customer, Guid> customerRepository,
        HallAvailabilityManager hallAvailabilityManager,
        ReservationSchedulingManager reservationSchedulingManager,
        IReservationNumberGenerator reservationNumberGenerator,
        IUnitOfWorkManager unitOfWorkManager,
        Volo.Abp.Guids.IGuidGenerator guidGenerator,
        IStringLocalizer<BanquetHallManagement.Localization.BanquetHallManagementResource> localizer)
    {
        _reservationRepository = reservationRepository;
        _hallRepository = hallRepository;
        _serviceRepository = serviceRepository;
        _reservationServiceRepository = reservationServiceRepository;
        _customerRepository = customerRepository;
        _hallAvailabilityManager = hallAvailabilityManager;
        _reservationSchedulingManager = reservationSchedulingManager;
        _reservationNumberGenerator = reservationNumberGenerator;
        _unitOfWorkManager = unitOfWorkManager;
        _guidGenerator = guidGenerator;
        _localizer = localizer;
    }

    public async Task<(Reservation Reservation, IReadOnlyList<Guid> ServiceIds)> CreateAsync(
        CreateUpdateReservationDto input)
    {
        ValidateReservationTimes(input);

        if (input.GuestsCount <= 0)
        {
            throw new UserFriendlyException(_localizer["Validation:GuestsCountRequired"]);
        }

        var hall = await _hallRepository.GetAsync(input.HallId);
        await _customerRepository.GetAsync(input.CustomerId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        if (input.GuestsCount > hall.Capacity)
        {
            throw new UserFriendlyException(_localizer["Validation:GuestsExceedCapacity"]);
        }

        await _reservationSchedulingManager.EnsureNoSchedulingConflictAsync(
            input.HallId,
            input.EventDate,
            input.StartTime,
            input.EndTime);

        var reservation = Reservation.Create(
            _guidGenerator.Create(),
            input.HallId,
            input.CustomerId,
            new TimeSlot(input.EventDate, input.StartTime, input.EndTime),
            input.GuestsCount);

        reservation.AssignReservationNumber(
            await _reservationNumberGenerator.GenerateAsync());

        await _reservationRepository.InsertAsync(reservation, autoSave: false);

        var services = await LoadRequestedServicesAsync(input.ServiceIds);
        var totalPrice = Reservation.CalculateTotalPrice(
            hall.PricePerHour,
            input.StartTime,
            input.EndTime,
            services.Select(service => service.Price));

        await InsertReservationServicesAsync(reservation.Id, services);

        reservation.FinalizeCreation(totalPrice);

        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return (reservation, services.Select(service => service.Id).ToList());
    }

    public async Task<(Reservation Reservation, IReadOnlyList<Guid> ServiceIds)> UpdateAsync(
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
            throw new UserFriendlyException(_localizer["Validation:GuestsCountRequired"]);
        }

        await _customerRepository.GetAsync(input.CustomerId);

        var hall = await _hallRepository.GetAsync(input.HallId);

        _hallAvailabilityManager.EnsureCanAcceptBookings(hall);

        if (input.GuestsCount > hall.Capacity)
        {
            throw new UserFriendlyException(_localizer["Validation:GuestsExceedCapacity"]);
        }

        var candidateStatus = reservation.Status is ReservationStatus.Confirmed
            or ReservationStatus.FullyPaid
            ? reservation.Status
            : ReservationStatus.Pending;

        await _reservationSchedulingManager.EnsureNoSchedulingConflictAsync(
            input.HallId,
            input.EventDate,
            input.StartTime,
            input.EndTime,
            reservation.Id,
            candidateStatus);

        var services = await LoadRequestedServicesAsync(input.ServiceIds);
        var totalPrice = Reservation.CalculateTotalPrice(
            hall.PricePerHour,
            input.StartTime,
            input.EndTime,
            services.Select(service => service.Price));

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
        await _unitOfWorkManager.Current!.SaveChangesAsync();

        return (reservation, services.Select(service => service.Id).ToList());
    }

    private async Task<List<Service>> LoadRequestedServicesAsync(List<Guid> serviceIds)
    {
        if (serviceIds == null || serviceIds.Count == 0)
        {
            return [];
        }

        return await _serviceRepository.GetListAsync(service => serviceIds.Contains(service.Id));
    }

    private async Task InsertReservationServicesAsync(
        Guid reservationId,
        IReadOnlyList<Service> services)
    {
        foreach (var service in services)
        {
            await _reservationServiceRepository.InsertAsync(
                new ReservationService(_guidGenerator.Create())
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
            link => link.ReservationId == reservationId);

        var requestedServiceIds = requestedServices
            .Select(service => service.Id)
            .ToHashSet();

        foreach (var link in existingLinks.Where(link => !requestedServiceIds.Contains(link.ServiceId)))
        {
            await _reservationServiceRepository.DeleteAsync(link, autoSave: false);
        }

        var existingServiceIds = existingLinks
            .Select(link => link.ServiceId)
            .ToHashSet();

        foreach (var service in requestedServices.Where(service => !existingServiceIds.Contains(service.Id)))
        {
            await _reservationServiceRepository.InsertAsync(
                new ReservationService(_guidGenerator.Create())
                {
                    ReservationId = reservationId,
                    ServiceId = service.Id
                },
                autoSave: false);
        }
    }

    private void ValidateReservationTimes(CreateUpdateReservationDto input)
    {
        if (input.StartTime >= input.EndTime)
        {
            throw new UserFriendlyException(_localizer["Validation:StartTimeBeforeEndTime"]);
        }
    }
}
