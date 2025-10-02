using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using AbpIdentityRole = Volo.Abp.Identity.IdentityRole;

namespace DeliveryApp.Domain.Data
{
    public class RoleSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IdentityRoleManager _roleManager;

        public RoleSeeder(
            IGuidGenerator guidGenerator,
            IdentityRoleManager roleManager)
        {
            _guidGenerator = guidGenerator;
            _roleManager = roleManager;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // Define all roles that should exist in the system
            var roles = new[]
            {
                new { Name = "admin", DisplayName = "Administrator", Description = "Full system administrator with all permissions" },
                new { Name = "manager", DisplayName = "Manager", Description = "System manager with administrative access" },
                new { Name = "restaurant_owner", DisplayName = "Restaurant Owner", Description = "Restaurant owner with restaurant management permissions" },
                new { Name = "delivery", DisplayName = "Delivery Person", Description = "Delivery person with order delivery permissions" },
                new { Name = "customer", DisplayName = "Customer", Description = "Regular customer with basic access" }
            };

            foreach (var roleInfo in roles)
            {
                await CreateRoleIfNotExistsAsync(roleInfo.Name, roleInfo.DisplayName, roleInfo.Description);
            }
        }

        private async Task CreateRoleIfNotExistsAsync(string roleName, string displayName, string description)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new AbpIdentityRole(_guidGenerator.Create(), roleName, null);
                role.SetProperty("DisplayName", displayName);
                role.SetProperty("Description", description);
                
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new System.Exception($"Failed to create role {roleName}: {string.Join(";", result.Errors)}");
                }
            }
            else
            {
                // Update display name and description if they're different
                var currentDisplayName = role.GetProperty<string>("DisplayName");
                var currentDescription = role.GetProperty<string>("Description");
                
                if (currentDisplayName != displayName || currentDescription != description)
                {
                    role.SetProperty("DisplayName", displayName);
                    role.SetProperty("Description", description);
                    await _roleManager.UpdateAsync(role);
                }
            }
        }
    }
}
