using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Reservations
{
    public interface IReservationAppService : IApplicationService
    {
        Task<ReservationDto> GetAsync(Guid id);

        Task<PagedResultDto<ReservationDto>> GetListAsync(
            PagedAndSortedResultRequestDto input);

        Task<ReservationDto> CreateAsync(
            CreateUpdateReservationDto input);

        Task<ReservationDto> UpdateAsync(
            Guid id,
            CreateUpdateReservationDto input);

        Task DeleteAsync(Guid id);
    }
}