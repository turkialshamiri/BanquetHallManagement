using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Identity;
using BanquetHallManagement.Permissions;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Identity;

public class BanquetHallManagementIdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionManager _permissionManager;

    public BanquetHallManagementIdentityDataSeedContributor(
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        ILookupNormalizer lookupNormalizer,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionManager permissionManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _lookupNormalizer = lookupNormalizer;
        _permissionDataSeeder = permissionDataSeeder;
        _permissionManager = permissionManager;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        await EnsureRoleExistsAsync(BanquetHallManagementRoleNames.Admin);
        await EnsureRoleExistsAsync(BanquetHallManagementRoleNames.Employee);

        await SeedRolePermissionsAsync(context);
        await SeedAdminUserAsync(context);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        var existing = await _roleManager.FindByNameAsync(roleName);
        if (existing != null)
        {
            return;
        }

        var role = new IdentityRole(Guid.NewGuid(), roleName);
        var result = await _roleManager.CreateAsync(role);
        result.CheckErrors();
    }

    private async Task SeedRolePermissionsAsync(DataSeedContext context)
    {
        var allPermissions = GetAllPermissions().ToArray();

        var adminPermissions = allPermissions;
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            BanquetHallManagementRoleNames.Admin,
            adminPermissions,
            context.TenantId
        );

        var employeePermissions = new[]
        {
            BanquetHallManagementPermissions.Dashboard.Default,

            BanquetHallManagementPermissions.Halls.Default,

            BanquetHallManagementPermissions.Customers.Default,
            BanquetHallManagementPermissions.Customers.Create,
            BanquetHallManagementPermissions.Customers.Update,
            BanquetHallManagementPermissions.Customers.Delete,

            BanquetHallManagementPermissions.Reservations.Default,
            BanquetHallManagementPermissions.Reservations.Create,
            BanquetHallManagementPermissions.Reservations.Update,
            BanquetHallManagementPermissions.Reservations.Delete,
            BanquetHallManagementPermissions.Reservations.Confirm,
            BanquetHallManagementPermissions.Reservations.Cancel,
            BanquetHallManagementPermissions.Reservations.Complete,
        };

        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            BanquetHallManagementRoleNames.Employee,
            employeePermissions,
            context.TenantId
        );

        var employeePermissionSet = employeePermissions.ToHashSet(StringComparer.Ordinal);
        foreach (var permission in allPermissions.Where(p => !employeePermissionSet.Contains(p)))
        {
            await _permissionManager.SetAsync(
                permission,
                RolePermissionValueProvider.ProviderName,
                BanquetHallManagementRoleNames.Employee,
                false
            );
        }
    }

    private static IEnumerable<string> GetAllPermissions()
    {
        yield return BanquetHallManagementPermissions.Dashboard.Default;
        yield return BanquetHallManagementPermissions.Dashboard.ViewRevenue;

        yield return BanquetHallManagementPermissions.Halls.Default;
        yield return BanquetHallManagementPermissions.Halls.Create;
        yield return BanquetHallManagementPermissions.Halls.Update;
        yield return BanquetHallManagementPermissions.Halls.Delete;

        yield return BanquetHallManagementPermissions.Customers.Default;
        yield return BanquetHallManagementPermissions.Customers.Create;
        yield return BanquetHallManagementPermissions.Customers.Update;
        yield return BanquetHallManagementPermissions.Customers.Delete;

        yield return BanquetHallManagementPermissions.Services.Default;
        yield return BanquetHallManagementPermissions.Services.Create;
        yield return BanquetHallManagementPermissions.Services.Update;
        yield return BanquetHallManagementPermissions.Services.Delete;

        yield return BanquetHallManagementPermissions.Reservations.Default;
        yield return BanquetHallManagementPermissions.Reservations.Create;
        yield return BanquetHallManagementPermissions.Reservations.Update;
        yield return BanquetHallManagementPermissions.Reservations.Delete;
        yield return BanquetHallManagementPermissions.Reservations.Confirm;
        yield return BanquetHallManagementPermissions.Reservations.Cancel;
        yield return BanquetHallManagementPermissions.Reservations.Complete;

        yield return BanquetHallManagementPermissions.Reports.Default;

        yield return BanquetHallManagementPermissions.Users.Default;
        yield return BanquetHallManagementPermissions.Users.Create;
        yield return BanquetHallManagementPermissions.Users.Update;
        yield return BanquetHallManagementPermissions.Users.Delete;
        yield return BanquetHallManagementPermissions.Users.ResetPassword;
        yield return BanquetHallManagementPermissions.Users.ManageRoles;
        yield return BanquetHallManagementPermissions.Users.Activate;
        yield return BanquetHallManagementPermissions.Users.Deactivate;
    }

    private async Task SeedAdminUserAsync(DataSeedContext context)
    {
        const string userName = "turkialshamiri";
        const string email = "turki@gmail.com";
        const string password = "turki4321";
        const string name = "Turki";
        const string surName = "Alshamiri";
        const string phoneNumber = "+967700000000";

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(email);
        }

        if (user == null)
        {
            user = new IdentityUser(Guid.NewGuid(), userName, email, context.TenantId)
            {
                Name = name,
                Surname = surName,
            };

            user.SetIsActive(true);

            (await _userManager.CreateAsync(user, password)).CheckErrors();

            (await _userManager.SetPhoneNumberAsync(user, phoneNumber)).CheckErrors();
        }
        else
        {
            var changed = false;

            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                (await _userManager.SetEmailAsync(user, email)).CheckErrors();
                changed = true;
            }

            if (!string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                (await _userManager.SetUserNameAsync(user, userName)).CheckErrors();
                changed = true;
            }

            if (user.Name != name)
            {
                user.Name = name;
                changed = true;
            }

            if (user.Surname != surName)
            {
                user.Surname = surName;
                changed = true;
            }

            if (user.PhoneNumber != phoneNumber)
            {
                (await _userManager.SetPhoneNumberAsync(user, phoneNumber)).CheckErrors();
                changed = true;
            }

            if (!user.IsActive)
            {
                user.SetIsActive(true);
                changed = true;
            }

            if (changed)
            {
                (await _userManager.UpdateAsync(user)).CheckErrors();
            }
        }

        if (!await _userManager.IsInRoleAsync(user, BanquetHallManagementRoleNames.Admin))
        {
            (await _userManager.AddToRoleAsync(user, BanquetHallManagementRoleNames.Admin)).CheckErrors();
        }

        // Ensure password matches requested seed (safe: reset only if password is different).
        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            (await _userManager.ResetPasswordAsync(user, resetToken, password)).CheckErrors();
        }
    }
}

