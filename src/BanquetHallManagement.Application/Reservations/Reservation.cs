using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
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

namespace BanquetHallManagement.Reservations
{
    public class ReservationAppService :
        ApplicationService,
        IReservationAppService
    {
        private readonly IRepository<Reservation, Guid> _reservationRepository;

        private readonly IRepository<Hall, Guid> _hallRepository;

        private readonly IRepository<Service, Guid> _serviceRepository;

        private readonly IRepository<ReservationService, Guid>
            _reservationServiceRepository;

        public ReservationAppService(
            IRepository<Reservation, Guid> reservationRepository,
            IRepository<Hall, Guid> hallRepository,
            IRepository<Service, Guid> serviceRepository,
            IRepository<ReservationService, Guid> reservationServiceRepository)
        {
            _reservationRepository = reservationRepository;

            _hallRepository = hallRepository;

            _serviceRepository = serviceRepository;

            _reservationServiceRepository =
                reservationServiceRepository;
        }

        public async Task<ReservationDto> GetAsync(Guid id)
        {
            var reservation =
                await _reservationRepository.GetAsync(id);

            return ObjectMapper.Map<
                Reservation,
                ReservationDto>(reservation);
        }

        public async Task<PagedResultDto<ReservationDto>>
            GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query =
                await _reservationRepository.GetQueryableAsync();

            var totalCount = query.Count();

            var reservations = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<ReservationDto>
            {
                TotalCount = totalCount,

                Items = ObjectMapper.Map<
                    List<Reservation>,
                    List<ReservationDto>>(reservations)
            };
        }

        public async Task<ReservationDto> CreateAsync(
            CreateUpdateReservationDto input)
        {
            // =========================
            // Validation
            // =========================

            if (input.StartTime >= input.EndTime)
            {
                throw new UserFriendlyException(
                    "وقت البداية يجب أن يكون أقل من وقت النهاية");
            }

            if (input.GuestsCount <= 0)
            {
                throw new UserFriendlyException(
                    "عدد الضيوف يجب أن يكون أكبر من صفر");
            }

            // =========================
            // Hall Validation
            // =========================

            var hall =
                await _hallRepository.GetAsync(input.HallId);

            if (hall == null)
            {
                throw new UserFriendlyException(
                    "القاعة غير موجودة");
            }

            if (input.GuestsCount > hall.Capacity)
            {
                throw new UserFriendlyException(
                    "عدد الضيوف يتجاوز سعة القاعة");
            }

            // =========================
            // Conflict Validation
            // =========================

            var query =
                await _reservationRepository.GetQueryableAsync();

            var hasConflict = query.Any(r =>
                r.HallId == input.HallId &&
                r.EventDate.Date == input.EventDate.Date &&
                r.Status != "Cancelled" &&
                input.StartTime < r.EndTime &&
                input.EndTime > r.StartTime
            );

            if (hasConflict)
            {
                throw new UserFriendlyException(
                    "هذه القاعة محجوزة بالفعل في هذا الوقت");
            }

            // =========================
            // Calculate Total Price
            // =========================

            var reservationHours =
                (input.EndTime - input.StartTime).TotalHours;

            decimal totalPrice =
                (decimal)reservationHours *
                hall.PricePerHour;

            // =========================
            // Create Reservation
            // =========================

            var reservation = new Reservation
            {
                HallId = input.HallId,

                CustomerId = input.CustomerId,

                EventDate = input.EventDate,

                StartTime = input.StartTime,

                EndTime = input.EndTime,

                GuestsCount = input.GuestsCount,

                Status = "Pending",

                TotalPrice = 0
            };

            await _reservationRepository.InsertAsync(
                reservation,
                autoSave: true);

            // =========================
            // Add Services
            // =========================

            if (input.ServiceIds != null &&
                input.ServiceIds.Any())
            {
                var services =
                    await _serviceRepository.GetListAsync(
                        s => input.ServiceIds.Contains(s.Id));

                totalPrice += services.Sum(s => s.Price);

                foreach (var service in services)
                {
                    await _reservationServiceRepository
                        .InsertAsync(
                            new ReservationService
                            {
                                ReservationId = reservation.Id,

                                ServiceId = service.Id
                            }
                        );
                }
            }

            // =========================
            // Update Total Price
            // =========================

            reservation.TotalPrice = totalPrice;

            await _reservationRepository.UpdateAsync(
                reservation);

            return ObjectMapper.Map<
                Reservation,
                ReservationDto>(reservation);
        }

        public async Task<ReservationDto> UpdateAsync(
            Guid id,
            CreateUpdateReservationDto input)
        {
            var reservation =
                await _reservationRepository.GetAsync(id);

            reservation.EventDate = input.EventDate;

            reservation.StartTime = input.StartTime;

            reservation.EndTime = input.EndTime;

            reservation.GuestsCount = input.GuestsCount;

            await _reservationRepository.UpdateAsync(
                reservation);

            return ObjectMapper.Map<
                Reservation,
                ReservationDto>(reservation);
        }

        public async Task DeleteAsync(Guid id)
        {
            var exists =
                await _reservationRepository.AnyAsync(
                    x => x.Id == id);

            if (!exists)
            {
                throw new UserFriendlyException(
                    "الحجز غير موجود");
            }

            await _reservationRepository.DeleteAsync(id);
        }
    }
}