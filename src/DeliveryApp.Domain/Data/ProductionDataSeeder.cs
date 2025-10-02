using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
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
    /// Production-ready data seeder that creates essential system data without sample/test data
    /// </summary>
    public class ProductionDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<RestaurantCategory, Guid> _categoryRepository;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly IRepository<SystemSetting, Guid> _systemSettingRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IPasswordHasher<AbpIdentityUser> _passwordHasher;

        public ProductionDataSeeder(
            IRepository<RestaurantCategory, Guid> categoryRepository,
            IRepository<AppUser, Guid> userRepository,
            IRepository<SystemSetting, Guid> systemSettingRepository,
            IGuidGenerator guidGenerator,
            IdentityUserManager userManager,
            IdentityRoleManager roleManager,
            IPasswordHasher<AbpIdentityUser> passwordHasher)
        {
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _systemSettingRepository = systemSettingRepository;
            _guidGenerator = guidGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // Only seed if no system settings exist (indicates fresh installation)
            if (await _systemSettingRepository.GetCountAsync() > 0)
            {
                return;
            }

            // Create application roles
            await CreateEssentialRolesAsync();

            // Create system settings
            await CreateSystemSettingsAsync();

            // Create essential restaurant categories
            await CreateEssentialCategoriesAsync();

            // Create users of each type if none exist
            await CreateUsersOfEachTypeAsync();
        }

        private async Task CreateEssentialRolesAsync()
        {
            // Create essential roles if they don't exist
            string[] roleNames = { "admin", "customer", "delivery", "restaurant_owner" };
            
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new AbpIdentityRole(_guidGenerator.Create(), roleName, null);
                    await _roleManager.CreateAsync(role);
                }
            }
        }

        private async Task CreateSystemSettingsAsync()
        {
            var systemSetting = new SystemSetting()
            {
                GeneralSettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    app_name = "وصيل",
                    app_version = "1.3.0",
                    currency = "SYP",
                    currency_symbol = "ل.س",
                    support_email = "support@waselsy.com",
                    support_phone = "+963123456789"
                }),
                DeliverySettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    default_delivery_fee = 5.00m,
                    max_delivery_distance = 15.0,
                    delivery_time_estimate = "30-45",
                    delivery_person_approval_required = true
                }),
                SecuritySettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    restaurant_approval_required = true,
                    maintenance_mode = false
                }),
                NotificationSettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    enable_reviews = true,
                    enable_special_offers = true,
                    enable_advertisements = true,
                    enable_chat_support = true
                }),
                MaintenanceSettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    tax_rate = 0.08m,
                    min_order_amount = 10.00m,
                    stripe_enabled = false,
                    cod_enabled = true
                }),
                Version = 1,
                IsActive = true
            };

            await _systemSettingRepository.InsertAsync(systemSetting);
        }

        private async Task CreateEssentialCategoriesAsync()
        {
            var desiredCategories = new List<(string name, string description, string imageUrl, int sortOrder)>
            {
                ("مطاعم", "مطاعم الوجبات السريعة والمأكولات", "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", 1),
                ("حلويات", "محلات الحلويات والمعجنات", "https://images.unsplash.com/photo-1579372786545-d24232daf58c", 2),
                ("مشروبات", "مقاهي ومحلات المشروبات", "https://images.unsplash.com/photo-1559056199-641a0ac8b55e", 3),
                ("صحية", "مطاعم الأكل الصحي والعضوي", "https://images.unsplash.com/photo-1512621776951-a57141f2eefd", 4)
            };

            var categoriesToInsert = new List<RestaurantCategory>();

            foreach (var (name, description, imageUrl, sortOrder) in desiredCategories)
            {
                var existing = await _categoryRepository.FirstOrDefaultAsync(c => c.Name == name);
                if (existing != null)
                {
                    // Already exists – skip insertion
                    continue;
                }

                var category = new RestaurantCategory(_guidGenerator.Create())
                {
                    Name = name,
                    Description = description,
                    ImageUrl = imageUrl,
                    IsActive = true,
                    SortOrder = sortOrder
                };

                categoriesToInsert.Add(category);
            }

            if (categoriesToInsert.Any())
            {
                await _categoryRepository.InsertManyAsync(categoriesToInsert);
            }
        }

        private async Task CreateUsersOfEachTypeAsync()
        {
            // Check if any users exist
            var existingUsers = await _userRepository.GetListAsync();
            if (existingUsers.Count > 0)
            {
                return; // Users already exist
            }

            // 1. Create Admin User
            await CreateAdminUserAsync();

            // 2. Create Customer User
            await CreateCustomerUserAsync();

            // 3. Create Delivery Person User
            await CreateDeliveryUserAsync();

            // 4. Create Restaurant Owner User
            await CreateRestaurantOwnerUserAsync();
        }

        private async Task CreateAdminUserAsync()
        {
            var adminUser = new AppUser(_guidGenerator.Create(), "admin@waselsy.com", "admin@waselsy.com")
            {
                Name = "مدير النظام",
                ProfileImageUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                IsEmailConfirmed = true,
                IsPhoneConfirmed = true,
                IsAdminApproved = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");
            
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.SetPhoneNumberAsync(adminUser, "+963123456789");
            await _userManager.AddToRoleAsync(adminUser, "admin");
        }

        private async Task CreateCustomerUserAsync()
        {
            var customerUser = new AppUser(_guidGenerator.Create(), "customer@waselsy.com", "customer@waselsy.com")
            {
                Name = "عميل تجريبي",
                ProfileImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                IsEmailConfirmed = true,
                IsPhoneConfirmed = true,
                IsAdminApproved = true
            };

            var result = await _userManager.CreateAsync(customerUser, "Customer123!");
            
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create customer user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.SetPhoneNumberAsync(customerUser, "+963912345678");
            await _userManager.AddToRoleAsync(customerUser, "customer");

            // Add sample address for customer
            var homeAddress = new Address(_guidGenerator.Create())
            {
                Title = "المنزل",
                Street = "شارع الروضة",
                City = "دمشق",
                State = "دمشق",
                ZipCode = "10001",
                FullAddress = "شارع الروضة، دمشق",
                Latitude = 33.5138,
                Longitude = 36.2765,
                IsDefault = true,
                UserId = customerUser.Id,
                User = customerUser
            };

            customerUser.Addresses = new List<Address> { homeAddress };

            // Add payment method for customer
            var cashPayment = new PaymentMethod(_guidGenerator.Create())
            {
                Type = PaymentType.CashOnDelivery,
                Title = "نقدي عند التوصيل",
                LastFourDigits = "",
                CardHolderName = "",
                ExpiryDate = "",
                IsDefault = true,
                UserId = customerUser.Id,
                User = customerUser
            };

            customerUser.PaymentMethods = new List<PaymentMethod> { cashPayment };

            // Add user preferences
            customerUser.Preferences = new UserPreferences
            {
                FavoriteCuisines = new List<string> { "مطاعم", "حلويات" },
                DietaryRestrictions = new List<string> { "لا يوجد" },
                NotificationSettings = new NotificationSettings
                {
                    OrderUpdates = true,
                    SpecialOffers = true,
                    NewsletterSubscription = false
                }
            };

            await _userManager.UpdateAsync(customerUser);
        }

        private async Task CreateDeliveryUserAsync()
        {
            var deliveryUser = new AppUser(_guidGenerator.Create(), "delivery@waselsy.com", "delivery@waselsy.com")
            {
                Name = "موظف توصيل",
                ProfileImageUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                IsEmailConfirmed = true,
                IsPhoneConfirmed = true,
                IsAdminApproved = true
            };

            var result = await _userManager.CreateAsync(deliveryUser, "Delivery123!");
            
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create delivery user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.SetPhoneNumberAsync(deliveryUser, "+963923456789");
            await _userManager.AddToRoleAsync(deliveryUser, "delivery");

            // Add delivery person location data
            deliveryUser.CurrentLocation = new Location
            {
                Latitude = 33.5150,
                Longitude = 36.2800,
                LastUpdated = DateTime.Now
            };

            // Add delivery status
            deliveryUser.DeliveryStatus = new DeliveryStatus
            {
                IsAvailable = true,
                CurrentOrderId = null,
                LastStatusUpdate = DateTime.Now
            };

            await _userManager.UpdateAsync(deliveryUser);
        }

        private async Task CreateRestaurantOwnerUserAsync()
        {
            var restaurantOwnerUser = new AppUser(_guidGenerator.Create(), "restaurant@waselsy.com", "restaurant@waselsy.com")
            {
                Name = "صاحب مطعم",
                ProfileImageUrl = "https://images.unsplash.com/photo-1560250097-0b93528c311a?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                IsEmailConfirmed = true,
                IsPhoneConfirmed = true,
                IsAdminApproved = true
            };

            var result = await _userManager.CreateAsync(restaurantOwnerUser, "Restaurant123!");
            
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create restaurant owner user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.SetPhoneNumberAsync(restaurantOwnerUser, "+963934567890");
            await _userManager.AddToRoleAsync(restaurantOwnerUser, "restaurant_owner");

            await _userManager.UpdateAsync(restaurantOwnerUser);
        }
    }
}
