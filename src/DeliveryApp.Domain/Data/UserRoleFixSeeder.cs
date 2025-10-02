using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AbpIdentityUser = Volo.Abp.Identity.IdentityUser;
using Volo.Abp.Guids;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Data
{
    public class UserRoleFixSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IRepository<AbpIdentityUser, Guid> _userRepository;

        public UserRoleFixSeeder(
            IdentityUserManager userManager,
            IdentityRoleManager roleManager,
            IGuidGenerator guidGenerator,
            IRepository<AbpIdentityUser, Guid> userRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _guidGenerator = guidGenerator;
            _userRepository = userRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // Ensure customer role exists
            if (!await _roleManager.RoleExistsAsync("customer"))
            {
                var customerRole = new IdentityRole(_guidGenerator.Create(), "customer", null);
                await _roleManager.CreateAsync(customerRole);
            }

            // Get all users using repository instead of UserManager.Users
            var allUsers = await _userRepository.GetListAsync();
            
            foreach (var user in allUsers)
            {
                // Check if user has any roles
                var userRoles = await _userManager.GetRolesAsync(user);
                
                // If user has no roles, assign them the "customer" role
                if (!userRoles.Any())
                {
                    await _userManager.AddToRoleAsync(user, "customer");
                    Console.WriteLine($"Assigned 'customer' role to user: {user.Email}");
                }
            }
        }
    }
} 
