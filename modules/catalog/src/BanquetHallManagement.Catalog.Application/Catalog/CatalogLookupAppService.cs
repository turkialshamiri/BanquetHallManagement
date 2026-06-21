using System;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Services;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Catalog;

public class CatalogLookupAppService : ApplicationService, ICatalogLookupAppService
{
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;

    public CatalogLookupAppService(
        IRepository<Hall, Guid> hallRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Service, Guid> serviceRepository)
    {
        _hallRepository = hallRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<HallLookupDto> GetHallAsync(Guid id)
    {
        var hall = await _hallRepository.GetAsync(id);

        return new HallLookupDto
        {
            Id = hall.Id,
            Name = hall.Name
        };
    }

    public async Task<bool> HallExistsAsync(Guid id)
    {
        return await _hallRepository.AnyAsync(x => x.Id == id);
    }

    public async Task<CustomerLookupDto> GetCustomerAsync(Guid id)
    {
        var customer = await _customerRepository.GetAsync(id);

        return new CustomerLookupDto
        {
            Id = customer.Id,
            Name = customer.Name
        };
    }

    public async Task<bool> CustomerExistsAsync(Guid id)
    {
        return await _customerRepository.AnyAsync(x => x.Id == id);
    }

    public async Task<ServiceLookupDto> GetServiceAsync(Guid id)
    {
        var service = await _serviceRepository.GetAsync(id);

        return new ServiceLookupDto
        {
            Id = service.Id,
            Name = service.Name
        };
    }

    public async Task<bool> ServiceExistsAsync(Guid id)
    {
        return await _serviceRepository.AnyAsync(x => x.Id == id);
    }
}
