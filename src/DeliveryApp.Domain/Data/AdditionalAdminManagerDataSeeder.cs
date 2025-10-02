using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using AbpIdentityRole = Volo.Abp.Identity.IdentityRole;
using AbpIdentityUser = Volo.Abp.Identity.IdentityUser;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Authorization.Permissions;

namespace DeliveryApp.Domain.Data
{
    // This seeder guarantees that demo admin & manager accounts exist so that the dashboard's
    // demo credentials (admin@example.com / admin123, manager@example.com / manager123) work.
    public class AdditionalAdminManagerDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IdentityRoleManager _roleManager;
        private readonly IdentityUserManager _userManager;
        private readonly IPasswordHasher<AbpIdentityUser> _passwordHasher;
        private readonly IPermissionDataSeeder _permissionDataSeeder;

        private const string AdminRoleName = "admin";
        private const string ManagerRoleName = "manager";

        private const string AdminEmail = "admin@example.com";
        private const string ManagerEmail = "manager@example.com";

        // IMPORTANT: In a real system you would NOT hard-code passwords. These are for demo only.
        private const string AdminPassword = "Admin123!";
        private const string ManagerPassword = "Manager123!";

        public AdditionalAdminManagerDataSeeder(
            IGuidGenerator guidGenerator,
            IdentityRoleManager roleManager,
            IdentityUserManager userManager,
            IPasswordHasher<AbpIdentityUser> passwordHasher,
            IPermissionDataSeeder permissionDataSeeder)
        {
            _guidGenerator = guidGenerator;
            _roleManager = roleManager;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _permissionDataSeeder = permissionDataSeeder;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // 1. Ensure roles exist
            await CreateRoleIfNotExistsAsync(AdminRoleName);
            await CreateRoleIfNotExistsAsync(ManagerRoleName);

            // 2. Grant permissions to roles
            await GrantPermissionsToRolesAsync(context);

            // 3. Ensure users exist and are assigned to roles
            var tenantId = context?.TenantId;

            await CreateUserIfNotExistsAsync(AdminEmail, AdminPassword, AdminRoleName, tenantId);
            await CreateUserIfNotExistsAsync(ManagerEmail, ManagerPassword, ManagerRoleName, tenantId);
        }

        private async Task GrantPermissionsToRolesAsync(DataSeedContext context)
        {
            // Grant ABP Identity permissions to admin role
            await _permissionDataSeeder.SeedAsync(
                RolePermissionValueProvider.ProviderName,
                AdminRoleName,
                new[] {
                    "AbpIdentity.Users",
                    "AbpIdentity.Users.Create",
                    "AbpIdentity.Users.Update",
                    "AbpIdentity.Users.Delete",
                    "AbpIdentity.Users.ManagePermissions",
                    "AbpIdentity.Roles",
                    "AbpIdentity.Roles.Create",
                    "AbpIdentity.Roles.Update",
                    "AbpIdentity.Roles.Delete",
                    "AbpIdentity.Roles.ManagePermissions",
                    "AbpIdentity.OrganizationUnits",
                    "AbpIdentity.OrganizationUnits.Create",
                    "AbpIdentity.OrganizationUnits.Update",
                    "AbpIdentity.OrganizationUnits.Delete",
                    "AbpIdentity.OrganizationUnits.ManagePermissions",
                    "AbpIdentity.OrganizationUnits.ManageUsers",
                    "AbpIdentity.OrganizationUnits.ManageRoles"
                },
                context?.TenantId
            );

            // Grant limited permissions to manager role
            await _permissionDataSeeder.SeedAsync(
                RolePermissionValueProvider.ProviderName,
                ManagerRoleName,
                new[] {
                    "AbpIdentity.Users",
                    "AbpIdentity.Users.Create",
                    "AbpIdentity.Users.Update",
                    "AbpIdentity.Roles"
                },
                context?.TenantId
            );
        }

        private async Task CreateRoleIfNotExistsAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new AbpIdentityRole(_guidGenerator.Create(), roleName, null);
                await _roleManager.CreateAsync(role);
            }
        }

        private async Task CreateUserIfNotExistsAsync(string email, string password, string roleName, Guid? tenantId)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Ensure user has the desired role
                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
                return;
            }

            user = new AbpIdentityUser(_guidGenerator.Create(), email, email, tenantId)
            {
                Name = email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new System.Exception("Failed to create demo user: " + string.Join(";", result.Errors));
            }

            // Confirm email directly without token generation
            user.SetEmailConfirmed(true);
            await _userManager.UpdateAsync(user);

            await _userManager.AddToRoleAsync(user, roleName);
        }
    }
} 
