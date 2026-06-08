using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Employees;

public interface IEmployeeAppService : IApplicationService
{
    Task<EmployeeDto> GetAsync(Guid id);

    Task<PagedResultDto<EmployeeDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<EmployeeDto> CreateAsync(CreateEmployeeDto input);

    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto input);

    Task DeleteAsync(Guid id);

    Task<EmployeeDto> ActivateAsync(Guid id);

    Task<EmployeeDto> DeactivateAsync(Guid id);

    Task ResetPasswordAsync(Guid id, ResetPasswordDto input);
}

