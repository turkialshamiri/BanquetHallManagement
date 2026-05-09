using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Reservations
{
    public interface IReservationAppService :
        ICrudAppService<
            ReservationDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateReservationDto>
    {

    }
}
