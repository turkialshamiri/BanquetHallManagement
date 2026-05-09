using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities;
using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;

namespace BanquetHallManagement
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class CustomerToCustomerDtoMapper : MapperBase<Customer, CustomerDto>
    {
        public override partial CustomerDto Map(Customer source);

        public override partial void Map(Customer source, CustomerDto destination);
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class CreateUpdateCustomerDtoToCustomerMapper
    : MapperBase<CreateUpdateCustomerDto, Customer>
    {
        public override partial Customer Map(CreateUpdateCustomerDto source);

        public override partial void Map(CreateUpdateCustomerDto source, Customer destination);
    }
}
