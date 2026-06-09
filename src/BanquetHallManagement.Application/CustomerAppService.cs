using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
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
        private readonly IRepository<Reservation, Guid> _reservationRepository;

        public CustomerAppService(
            IRepository<Customer, Guid> repository,
            IRepository<Reservation, Guid> reservationRepository)
            : base(repository)
        {
            _reservationRepository = reservationRepository;
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
        public override async Task DeleteAsync(Guid id)
        {
            await Repository.GetAsync(id);

            var hasReservations = await _reservationRepository.AnyAsync(r => r.CustomerId == id);
            if (hasReservations)
            {
                throw new BusinessException(
                    BanquetHallManagementDomainErrorCodes.CustomerCannotDeleteHasReservations);
            }

            await base.DeleteAsync(id);
        }
    }
}