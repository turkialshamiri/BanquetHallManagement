using BanquetHallManagement.Entities;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
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
        CrudAppService<
            Reservation,
            ReservationDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateReservationDto>,
        IReservationAppService
    {
        private readonly IRepository<Hall, Guid> _hallRepo;
        private readonly IRepository<Service, Guid> _serviceRepo;
        private readonly IRepository<ReservationService, Guid> _reservationServiceRepo;

        public ReservationAppService(
            IRepository<Reservation, Guid> repository,
            IRepository<Hall, Guid> hallRepo,
            IRepository<Service, Guid> serviceRepo)
            : base(repository)
        {
            _hallRepo = hallRepo;
            _serviceRepo = serviceRepo;
        }

        public override async Task<ReservationDto> CreateAsync(CreateUpdateReservationDto input)
        {
            //  جلب القاعة
            var hall = await _hallRepo.GetAsync(input.HallId);

            if (hall == null)
                throw new UserFriendlyException("Hall not found");

            // منع التعارض (Booking Conflict)
            var query = await Repository.GetQueryableAsync();

            var hasConflict = query.Any(r =>
                r.HallId == input.HallId &&
                r.EventDate == input.EventDate &&
                r.Status != "Cancelled" &&
                (input.StartTime < r.EndTime &&
                 input.EndTime > r.StartTime)
            );

            if (hasConflict)
            {
                throw new UserFriendlyException("هذه القاعة محجوزة في هذا الوقت");
            }

            //  إنشاء الحجز
            var reservation = new Reservation
            {
                HallId = input.HallId,
                CustomerId = input.CustomerId,
                EventDate = input.EventDate,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                GuestsCount = input.GuestsCount,
                Status = "Pending"
            };

            //  حساب السعر الأساسي (Hall)
            var hours = (input.EndTime - input.StartTime).TotalHours;
            var total = (decimal)hours * hall.PricePerHour;

            //  إضافة الخدمات
            if (input.ServiceIds != null && input.ServiceIds.Any())
            {
                var services = await _serviceRepo.GetListAsync(
                    s => input.ServiceIds.Contains(s.Id)
                );

                total += services.Sum(s => s.Price);

                //  (اختياري احترافي) ربط الخدمات بالحجز
                foreach (var service in services)
                {
                    await _reservationServiceRepo.InsertAsync(
                        new ReservationService
                        {
                            ReservationId = reservation.Id,
                            ServiceId = service.Id
                        }
                    );
                }
            }

            reservation.TotalPrice = total;

            // حفظ الحجز
            await Repository.InsertAsync(reservation);

            // الإرجاع
            return ObjectMapper.Map<Reservation, ReservationDto>(reservation);
        }
    }
}