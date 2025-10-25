using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using AbpIdentityUser = Volo.Abp.Identity.IdentityUser;
using AbpIdentityRole = Volo.Abp.Identity.IdentityRole;

namespace DeliveryApp.Domain.Data
{
    /// <summary>
    /// Adds a couple of extra restaurant-owner users (and a simple restaurant for each)
    /// so that admin can test approval / rejection flows without conflicting
    /// with the main <see cref="SampleDataSeeder"/>.
    /// </summary>
    public class AdditionalRestaurantOwnersSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;

        public AdditionalRestaurantOwnersSeeder(
            IGuidGenerator guidGenerator,
            IdentityUserManager userManager,
            IdentityRoleManager roleManager,
            IRepository<AppUser, Guid> userRepository,
            IRepository<Restaurant, Guid> restaurantRepository)
        {
            _guidGenerator = guidGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // Ensure the restaurant_owner role exists
            await EnsureRoleExistsAsync("restaurant_owner");

            // Define friendly restaurant-owner seed data
            var owners = new List<(string Email,string Name,string Phone,string Restaurant)>
            {
                ("owner1@waselsy.com", "محمد مطعم", "+963912345678", "مأكولات الشام"),
                ("owner2@waselsy.com", "ليلى الأطباق", "+963933221144", "ليلى للمشاوي")
            };

            foreach (var (email, name, phone, restaurantName) in owners)
            {
                var existing = await _userManager.FindByEmailAsync(email);
                AppUser owner;
                if (existing == null)
                {
                    owner = new AppUser(_guidGenerator.Create(), email, email)
                    {
                        Name = name,
                        ProfileImageUrl = string.Empty,
                        // confirmation/admin flags
                        IsEmailConfirmed = true,
                        IsPhoneConfirmed = true,
                        IsAdminApproved = false
                    };

                    var creationResult = await _userManager.CreateAsync(owner, "Owner123!");
                    if (!creationResult.Succeeded)
                    {
                        throw new Exception($"Failed to create seed owner {email}: {string.Join(";", creationResult.Errors.Select(e=>e.Description))}");
                    }

                    await _userManager.AddToRoleAsync(owner, "restaurant_owner");
                }
                else
                {
                    owner = existing as AppUser;
                    await _userManager.AddToRoleAsync(owner, "restaurant_owner");
                }

                // Create a minimal restaurant for the owner if it doesn't exist
                var alreadyHasRestaurant = await _restaurantRepository.AnyAsync(r => r.OwnerId == owner.Id);
                if (!alreadyHasRestaurant)
                {
                    var restaurant = new Restaurant(_guidGenerator.Create())
                    {
                        Name = restaurantName,
                        Description = "مطعم تجريبي لأغراض الاختبار",
                        DeliveryTime = "30-40",
                        DeliveryFee = 0,
                        MinimumOrderAmount = 0,
                        OwnerId = owner.Id,
                        Address = new Address(_guidGenerator.Create())
                        {
                            Street = "مركز المدينة",
                            City = "دمشق",
                            State = "دمشق",
                            ZipCode = "0000",
                            FullAddress = "دمشق – مركز المدينة",
                            Latitude = 33.5138,
                            Longitude = 36.2765,
                            IsDefault = false
                        }
                    };

                    await _restaurantRepository.InsertAsync(restaurant);
                }
            }
        }

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new AbpIdentityRole(_guidGenerator.Create(), roleName, null);
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role {roleName}: {string.Join(";", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
