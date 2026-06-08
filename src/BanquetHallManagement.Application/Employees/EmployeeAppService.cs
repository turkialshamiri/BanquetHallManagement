using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Identity;
using BanquetHallManagement.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace BanquetHallManagement.Employees;

[Authorize(BanquetHallManagementPermissions.Users.Default)]
public class EmployeeAppService : ApplicationService, IEmployeeAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;

    public EmployeeAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager userManager,
        IdentityRoleManager roleManager)
    {
        _userRepository = userRepository;
        _identityUserRepository = identityUserRepository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<EmployeeDto> GetAsync(Guid id)
    {
        var user = await _userManager.GetByIdAsync(id);
        return await MapToEmployeeDtoAsync(user);
    }

    public async Task<PagedResultDto<EmployeeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _userRepository.GetQueryableAsync();

        var totalCount = queryable.Count();

        var users = queryable
            .OrderByDescending(u => u.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var items = new List<EmployeeDto>(users.Count);
        foreach (var user in users)
        {
            items.Add(await MapToEmployeeDtoAsync(user));
        }

        return new PagedResultDto<EmployeeDto>(totalCount, items);
    }

    [Authorize(BanquetHallManagementPermissions.Users.Create)]
    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto input)
    {
        if (string.IsNullOrWhiteSpace(input.UserName))
        {
            throw new UserFriendlyException("اسم المستخدم مطلوب");
        }

        if (string.IsNullOrWhiteSpace(input.Password))
        {
            throw new UserFriendlyException("كلمة المرور مطلوبة");
        }

        var roleName = NormalizeRole(input.Role);
        await EnsureRoleExistsAsync(roleName);

        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, CurrentTenant.Id)
        {
            Name = input.Name ?? string.Empty,
            Surname = input.Surname ?? string.Empty,
        };
        user.SetIsActive(true);

        EnsureSucceeded(await _userManager.CreateAsync(user, input.Password));

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            EnsureSucceeded(await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber));
        }

        EnsureSucceeded(await _userManager.AddToRoleAsync(user, roleName));

        await CurrentUnitOfWork.SaveChangesAsync();

        var persistedUser = await _userManager.GetByIdAsync(user.Id);
        return await MapToEmployeeDtoAsync(persistedUser);
    }

    [Authorize(BanquetHallManagementPermissions.Users.Update)]
    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto input)
    {
        var user = await _userManager.GetByIdAsync(id);

        var roleName = NormalizeRole(input.Role);
        await EnsureRoleExistsAsync(roleName);

        EnsureSucceeded(await _userManager.SetEmailAsync(user, input.Email));

        user.Name = input.Name ?? string.Empty;
        user.Surname = input.Surname ?? string.Empty;
        user.SetIsActive(input.IsActive);

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            EnsureSucceeded(await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber));
        }

        EnsureSucceeded(await _userManager.UpdateAsync(user));

        await ReplaceUserRoleAsync(user, roleName);

        await CurrentUnitOfWork.SaveChangesAsync();

        var persistedUser = await _userManager.GetByIdAsync(user.Id);
        return await MapToEmployeeDtoAsync(persistedUser);
    }

    [Authorize(BanquetHallManagementPermissions.Users.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var user = await _userManager.GetByIdAsync(id);
        EnsureSucceeded(await _userManager.DeleteAsync(user));
    }

    [Authorize(BanquetHallManagementPermissions.Users.Activate)]
    public async Task<EmployeeDto> ActivateAsync(Guid id)
    {
        var user = await _userManager.GetByIdAsync(id);
        user.SetIsActive(true);
        EnsureSucceeded(await _userManager.UpdateAsync(user));
        return await MapToEmployeeDtoAsync(user);
    }

    [Authorize(BanquetHallManagementPermissions.Users.Deactivate)]
    public async Task<EmployeeDto> DeactivateAsync(Guid id)
    {
        var user = await _userManager.GetByIdAsync(id);
        user.SetIsActive(false);
        EnsureSucceeded(await _userManager.UpdateAsync(user));
        return await MapToEmployeeDtoAsync(user);
    }

    [Authorize(BanquetHallManagementPermissions.Users.ResetPassword)]
    public async Task ResetPasswordAsync(Guid id, ResetPasswordDto input)
    {
        if (string.IsNullOrWhiteSpace(input.NewPassword))
        {
            throw new UserFriendlyException("كلمة المرور الجديدة مطلوبة");
        }

        var user = await _userManager.GetByIdAsync(id);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        EnsureSucceeded(await _userManager.ResetPasswordAsync(user, token, input.NewPassword));
    }

    private static string NormalizeRole(string? role)
    {
        return string.Equals(role, BanquetHallManagementRoleNames.Admin, StringComparison.OrdinalIgnoreCase)
            ? BanquetHallManagementRoleNames.Admin
            : BanquetHallManagementRoleNames.Employee;
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await _roleManager.FindByNameAsync(roleName) != null)
        {
            return;
        }

        var role = new IdentityRole(GuidGenerator.Create(), roleName);
        EnsureSucceeded(await _roleManager.CreateAsync(role));
    }

    private async Task ReplaceUserRoleAsync(IdentityUser user, string desiredRoleName)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        var hasDesired = currentRoles.Any(r => string.Equals(r, desiredRoleName, StringComparison.OrdinalIgnoreCase));

        if (hasDesired && currentRoles.Count == 1)
        {
            return;
        }

        foreach (var role in currentRoles.Where(r => !string.Equals(r, desiredRoleName, StringComparison.OrdinalIgnoreCase)))
        {
            EnsureSucceeded(await _userManager.RemoveFromRoleAsync(user, role));
        }

        if (!hasDesired)
        {
            EnsureSucceeded(await _userManager.AddToRoleAsync(user, desiredRoleName));
        }
    }

    private async Task<EmployeeDto> MapToEmployeeDtoAsync(IdentityUser user)
    {
        var roleNames = await _identityUserRepository.GetRoleNamesAsync(user.Id);

        return new EmployeeDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Surname = user.Surname ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActive = user.IsActive,
            Roles = roleNames.ToList(),
        };
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var message = string.Join(" | ", result.Errors.Select(e => e.Description));
        throw new UserFriendlyException(message);
    }
}
