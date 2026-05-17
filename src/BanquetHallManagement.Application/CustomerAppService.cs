using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Customers
{
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
    }
}