using BanquetHallManagement.Reservations;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ReservationToReservationDtoMapper : MapperBase<Reservation, ReservationDto>
{
    [MapperIgnoreTarget(nameof(ReservationDto.Status))]
    [MapperIgnoreTarget(nameof(ReservationDto.ServiceIds))]
    public override partial ReservationDto Map(Reservation source);

    [MapperIgnoreTarget(nameof(ReservationDto.Status))]
    [MapperIgnoreTarget(nameof(ReservationDto.ServiceIds))]
    public override partial void Map(Reservation source, ReservationDto destination);
}
