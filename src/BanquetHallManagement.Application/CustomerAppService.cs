using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Customers
{
    [Authorize(BanquetHallManagementPermissions.Customers.Default)]
    public class CustomerAppService :
        CrudAppService<
            Customer,
            CustomerDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateCustomerDto>,
        ICustomerAppService
    {
        public CustomerAppService(IRepository<Customer, Guid> repository)
            : base(repository)
        {
        }

        //  البحث برقم الهاتف
        public async Task<CustomerDto?> GetByPhoneAsync(string phone)
        {
            var query = await Repository.GetQueryableAsync();

            var customer = query.FirstOrDefault(x => x.Phone == phone);

            return customer == null
                ? null
                : ObjectMapper.Map<Customer, CustomerDto>(customer);
        }

        [Authorize(BanquetHallManagementPermissions.Customers.Create)]
        public override Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
        {
            return base.CreateAsync(input);
        }

        [Authorize(BanquetHallManagementPermissions.Customers.Update)]
        public override Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
        {
            return base.UpdateAsync(id, input);
        }

        [Authorize(BanquetHallManagementPermissions.Customers.Delete)]
        public override Task DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }
    }
}