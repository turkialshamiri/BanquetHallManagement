using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace BanquetHallManagement.Employees;

public class EmployeeDto : EntityDto<Guid>
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

