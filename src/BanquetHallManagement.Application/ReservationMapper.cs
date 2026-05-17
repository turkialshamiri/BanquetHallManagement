using BanquetHallManagement.Reservations;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ReservationToReservationDtoMapper : MapperBase<Reservation, ReservationDto>
{
    public override partial ReservationDto Map(Reservation source);

    public override partial void Map(Reservation source, ReservationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateReservationDtoToReservationMapper
    : MapperBase<CreateUpdateReservationDto, Reservation>
{
    public override partial Reservation Map(CreateUpdateReservationDto source);

    public override partial void Map(CreateUpdateReservationDto source, Reservation destination);
}