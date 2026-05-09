using BanquetHallManagement.Entities;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Halls;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class HallToHallDtoMapper : MapperBase<Hall, HallDto>
{
    public override partial HallDto Map(Hall source);

    public override partial void Map(Hall source, HallDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateHallDtoToHallMapper
    : MapperBase<CreateUpdateHallDto, Hall>
{
    public override partial Hall Map(CreateUpdateHallDto source);

    public override partial void Map(
        CreateUpdateHallDto source,
        Hall destination
    );
}