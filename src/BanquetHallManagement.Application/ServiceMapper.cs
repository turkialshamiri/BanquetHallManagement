using BanquetHallManagement.Entities;
using BanquetHallManagement.Services;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ServiceToServiceDtoMapper : MapperBase<Service, ServiceDto>
{
    public override partial ServiceDto Map(Service source);
    public override partial void Map(Service source, ServiceDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateServiceDtoToServiceMapper
    : MapperBase<CreateUpdateServiceDto, Service>
{
    public override partial Service Map(CreateUpdateServiceDto source);
    public override partial void Map(CreateUpdateServiceDto source, Service destination);
}