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
    public class SampleDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly IRepository<RestaurantCategory, Guid> _categoryRepository;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly IRepository<Order, Guid> _orderRepository;
        private readonly IRepository<Review, Guid> _reviewRepository;
        private readonly IRepository<SpecialOffer, Guid> _offerRepository;
        private readonly IRepository<Advertisement, Guid> _advertisementRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly IPasswordHasher<AbpIdentityUser> _passwordHasher;

        // Store created entities for reference
        private readonly Dictionary<string, Guid> _restaurantIds = new Dictionary<string, Guid>();
        private readonly Dictionary<string, Guid> _categoryIds = new Dictionary<string, Guid>();
        private readonly Dictionary<string, Guid> _userIds = new Dictionary<string, Guid>();
        private readonly Dictionary<string, Guid> _menuItemIds = new Dictionary<string, Guid>();
        private readonly Dictionary<string, Guid> _addressIds = new Dictionary<string, Guid>();
        private readonly Dictionary<string, Guid> _paymentMethodIds = new Dictionary<string, Guid>();
        private readonly Dictionary<string, Guid> _orderIds = new Dictionary<string, Guid>();

        public SampleDataSeeder(
            IRepository<Restaurant, Guid> restaurantRepository,
            IRepository<RestaurantCategory, Guid> categoryRepository,
            IRepository<AppUser, Guid> userRepository,
            IRepository<Order, Guid> orderRepository,
            IRepository<Review, Guid> reviewRepository,
            IRepository<SpecialOffer, Guid> offerRepository,
            IRepository<Advertisement, Guid> advertisementRepository,
            IGuidGenerator guidGenerator,
            IdentityUserManager userManager,
            IdentityRoleManager roleManager,
            IPasswordHasher<AbpIdentityUser> passwordHasher)
        {
            _restaurantRepository = restaurantRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _reviewRepository = reviewRepository;
            _offerRepository = offerRepository;
            _advertisementRepository = advertisementRepository;
            _guidGenerator = guidGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // Only seed if no restaurants exist (prevents duplicate seeding)
            if (await _restaurantRepository.GetCountAsync() > 0)
            {
                return;
            }

            // Create application roles
            await CreateRolesAsync();

            // Create sample users (three types: customer, delivery, restaurant owner)
            await CreateSampleUsersAsync();

            // Create restaurant categories
            await CreateSampleCategoriesAsync();

            // Create sample restaurants
            await CreateSampleRestaurantsAsync();

            // Create sample orders
            await CreateSampleOrdersAsync();
            
            // Create sample reviews
            await CreateSampleReviewsAsync();
            
            // Create special offers
            await CreateSpecialOffersAsync();
            
            // Create sample advertisements
            await CreateSampleAdvertisementsAsync();
        }

        private async Task CreateRolesAsync()
        {
            // Create roles if they don't exist
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

        private async Task CreateSampleUsersAsync()
        {
            // 1. Admin User
            await CreateAdminUserAsync();
            
            // 2. Customer User
            await CreateCustomerUserAsync();
            
            // 3. Delivery Person User
            await CreateDeliveryUserAsync();
            
            // 4. Restaurant Owner User
            await CreateRestaurantOwnerUserAsync();
        }

        private async Task CreateAdminUserAsync()
        {
            var existing = await _userManager.FindByEmailAsync("admin@waselsy.com");

            if (existing != null)
            {
                // User already exists, just remember its id and ensure role assignment
                await _userManager.AddToRoleAsync(existing, "admin");
                _userIds["admin"] = existing.Id;
                return;
            }

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
                var duplicateCodes = new[] { "DuplicateUserName", "DuplicateEmail" };
                if (!result.Succeeded && result.Errors.All(e => duplicateCodes.Contains(e.Code)))
                {
                    var existingUser = await _userManager.FindByEmailAsync("admin@waselsy.com");
                    if (existingUser != null)
                    {
                        await _userManager.AddToRoleAsync(existingUser, "admin");
                        _userIds["admin"] = existingUser.Id;
                        return;
                    }
                }
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.SetPhoneNumberAsync(adminUser, "+963123456789");
            await _userManager.AddToRoleAsync(adminUser, "admin");
            _userIds["admin"] = adminUser.Id;
        }

        private async Task CreateCustomerUserAsync()
        {
            var existing = await _userManager.FindByEmailAsync("customer@waselsy.com");
            if (existing != null)
            {
                await _userManager.AddToRoleAsync(existing, "customer");
                _userIds["customer"] = existing.Id;
            }
            else
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
                _userIds["customer"] = customerUser.Id;
            }

            // Add addresses for customer
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
                UserId = _userIds["customer"],
                User = await _userRepository.GetAsync(_userIds["customer"])
            };
            _addressIds["home"] = homeAddress.Id;

            var workAddress = new Address(_guidGenerator.Create())
            {
                Title = "العمل",
                Street = "شارع بغداد",
                City = "دمشق",
                State = "دمشق",
                ZipCode = "10002",
                FullAddress = "شارع بغداد، دمشق",
                Latitude = 33.5224,
                Longitude = 36.2922,
                IsDefault = false,
                UserId = _userIds["customer"],
                User = await _userRepository.GetAsync(_userIds["customer"])
            };
            _addressIds["work"] = workAddress.Id;

            var customer = await _userRepository.GetAsync(_userIds["customer"]);
            customer.Addresses = new List<Address> { homeAddress, workAddress };
            await _userRepository.UpdateAsync(customer);

            // Add payment methods for customer
            var creditCard = new PaymentMethod(_guidGenerator.Create())
            {
                Type = PaymentType.CreditCard,
                Title = "فيزا",
                LastFourDigits = "4242",
                CardHolderName = "محمد أحمد",
                ExpiryDate = "12/25",
                IsDefault = true,
                UserId = _userIds["customer"],
                User = await _userRepository.GetAsync(_userIds["customer"])
            };
            _paymentMethodIds["credit"] = creditCard.Id;

            var cashPayment = new PaymentMethod(_guidGenerator.Create())
            {
                Type = PaymentType.CashOnDelivery,
                Title = "نقدي",
                LastFourDigits = "",
                CardHolderName = "",
                ExpiryDate = "",
                IsDefault = false,
                UserId = _userIds["customer"],
                User = await _userRepository.GetAsync(_userIds["customer"])
            };
            _paymentMethodIds["cash"] = cashPayment.Id;

            customer = await _userRepository.GetAsync(_userIds["customer"]);
            customer.PaymentMethods = new List<PaymentMethod> { creditCard, cashPayment };
            await _userRepository.UpdateAsync(customer);
            
            // Add user preferences
            customer = await _userRepository.GetAsync(_userIds["customer"]);
            if (customer.Preferences == null)
            {
                customer.Preferences = new UserPreferences(_guidGenerator.Create())
                {
                    UserId = customer.Id,
                    User = customer,
                    FavoriteCuisines = new List<string> { "شاورما", "بيتزا", "حلويات" },
                    DietaryRestrictions = new List<string> { "لا يوجد" },
                    NotificationSettings = new NotificationSettings(_guidGenerator.Create())
                    {
                        OrderUpdates = true,
                        SpecialOffers = true,
                        NewsletterSubscription = false
                    }
                };
                await _userRepository.UpdateAsync(customer);
            }
        }

        private async Task CreateDeliveryUserAsync()
        {
            var existing = await _userManager.FindByEmailAsync("delivery@waselsy.com");
            if (existing != null)
            {
                await _userManager.AddToRoleAsync(existing, "delivery");
                _userIds["delivery"] = existing.Id;
            }
            else
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
                _userIds["delivery"] = deliveryUser.Id;
            }

            // Get the delivery user (either existing or newly created)
            var deliveryUserForUpdate = await _userManager.FindByEmailAsync("delivery@waselsy.com") as AppUser;
            
            if (deliveryUserForUpdate != null)
            {
                // Only add location and delivery status if they don't already exist
                if (deliveryUserForUpdate.CurrentLocation == null)
                {
                    deliveryUserForUpdate.CurrentLocation = new Location(_guidGenerator.Create())
                    {
                        Latitude = 33.5150,
                        Longitude = 36.2800,
                        LastUpdated = DateTime.Now
                    };
                }

                if (deliveryUserForUpdate.DeliveryStatus == null)
                {
                    deliveryUserForUpdate.DeliveryStatus = new DeliveryStatus(deliveryUserForUpdate.Id)
                    {
                        IsAvailable = true,
                        CurrentOrderId = null,
                        LastStatusUpdate = DateTime.Now
                    };
                }

                await _userManager.UpdateAsync(deliveryUserForUpdate);
            }
        }

        private async Task CreateRestaurantOwnerUserAsync()
        {
            var existing = await _userManager.FindByEmailAsync("restaurant@waselsy.com");
            if (existing != null)
            {
                await _userManager.AddToRoleAsync(existing, "restaurant_owner");
                _userIds["restaurant_owner"] = existing.Id;
            }
            else
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
                _userIds["restaurant_owner"] = restaurantOwnerUser.Id;
            }

            // Get the restaurant owner user (either existing or newly created)
            var restaurantOwnerUserForUpdate = await _userManager.FindByEmailAsync("restaurant@waselsy.com");
            if (restaurantOwnerUserForUpdate != null)
            {
                await _userManager.UpdateAsync(restaurantOwnerUserForUpdate);
            }
            
            // 4. Second Customer User (for more variety)
            var existingSecondCustomer = await _userManager.FindByEmailAsync("customer2@example.com");
            if (existingSecondCustomer != null)
            {
                await _userManager.AddToRoleAsync(existingSecondCustomer, "customer");
                _userIds["customer2"] = existingSecondCustomer.Id;
            }
            else
            {
                var secondCustomerUser = new AppUser(_guidGenerator.Create(), "customer2@example.com", "customer2@example.com")
                {
                    Name = "ليلى حسن",
                    ProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
                };
                var secondCustomerResult = await _userManager.CreateAsync(
                    secondCustomerUser, 
                    "Customer456!"
                );
                
                if (!secondCustomerResult.Succeeded)
                {
                    throw new Exception($"Failed to create second customer user: {string.Join(", ", secondCustomerResult.Errors.Select(e => e.Description))}");
                }
                
                await _userManager.SetPhoneNumberAsync(secondCustomerUser, "+963912345679");
                
                await _userManager.AddToRoleAsync(secondCustomerUser, "customer");
                _userIds["customer2"] = secondCustomerUser.Id;
            }
            
            // Add address for second customer
            var secondCustomerAddress = new Address(_guidGenerator.Create())
            {
                Title = "المنزل",
                Street = "شارع المزة",
                City = "دمشق",
                State = "دمشق",
                ZipCode = "10005",
                FullAddress = "شارع المزة، دمشق",
                Latitude = 33.5000,
                Longitude = 36.2500,
                IsDefault = true,
                UserId = _userIds["customer2"],
                User = await _userRepository.GetAsync(_userIds["customer2"])
            };
            _addressIds["home2"] = secondCustomerAddress.Id;
            
            var secondCustomerUserForUpdate = await _userRepository.GetAsync(_userIds["customer2"]);
            secondCustomerUserForUpdate.Addresses = new List<Address> { secondCustomerAddress };
            await _userRepository.UpdateAsync(secondCustomerUserForUpdate);
            
            // Add payment method for second customer
            var secondCustomerPayment = new PaymentMethod(_guidGenerator.Create())
            {
                Type = PaymentType.CreditCard,
                Title = "ماستركارد",
                LastFourDigits = "8888",
                CardHolderName = "ليلى حسن",
                ExpiryDate = "10/26",
                IsDefault = true,
                UserId = _userIds["customer2"],
                User = await _userRepository.GetAsync(_userIds["customer2"])
            };
            _paymentMethodIds["credit2"] = secondCustomerPayment.Id;
            
            var secondCustomerUserForPayment = await _userRepository.GetAsync(_userIds["customer2"]);
            secondCustomerUserForPayment.PaymentMethods = new List<PaymentMethod> { secondCustomerPayment };
            await _userRepository.UpdateAsync(secondCustomerUserForPayment);
            
            // Add preferences for second customer
            var secondCustomerUserForPreferences = await _userRepository.GetAsync(_userIds["customer2"]);
            if (secondCustomerUserForPreferences.Preferences == null)
            {
                secondCustomerUserForPreferences.Preferences = new UserPreferences(_guidGenerator.Create())
                {
                    UserId = secondCustomerUserForPreferences.Id,
                    User = secondCustomerUserForPreferences,
                    FavoriteCuisines = new List<string> { "آسيوي", "حلويات" },
                    DietaryRestrictions = new List<string> { "نباتي" },
                    NotificationSettings = new NotificationSettings(_guidGenerator.Create())
                    {
                        OrderUpdates = true,
                        SpecialOffers = false,
                        NewsletterSubscription = true
                    }
                };
                await _userRepository.UpdateAsync(secondCustomerUserForPreferences);
            }

            // Create an un-approved restaurant owner for testing admin approval flow
            var unapprovedOwnerId = _guidGenerator.Create();
            var unapprovedOwnerUser = new AppUser(unapprovedOwnerId, "unapproved@deliveryapp.com", "unapproved@deliveryapp.com")
            {
                Name = "محمد غير مفعل",
                IsEmailConfirmed = false,
                IsPhoneConfirmed = false,
                IsAdminApproved = false, // This user awaits admin approval
                ProfileImageUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80"
            };
            
            // Set phone number using ABP method
            unapprovedOwnerUser.SetPhoneNumber("+966501234567", false);

            var unapprovedResult = await _userManager.CreateAsync(unapprovedOwnerUser, "123Qwe!");
            if (!unapprovedResult.Succeeded)
            {
                throw new Exception($"Failed to create unapproved owner user: {string.Join(", ", unapprovedResult.Errors.Select(e => e.Description))}");
            }

            await _userManager.AddToRoleAsync(unapprovedOwnerUser, "restaurant_owner");
            _userIds["unapproved_owner"] = unapprovedOwnerUser.Id;

            await _userManager.UpdateAsync(unapprovedOwnerUser);
        }

        private async Task CreateSampleCategoriesAsync()
        {
            // Prepare desired category definitions
            var desiredCategories = new List<(string key, string name, string description, string imageUrl, int sortOrder)>
            {
                ("fast_food", "مطاعم", "مطاعم الوجبات السريعة", "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", 1),
                ("sweets", "حلويات", "محلات الحلويات والمعجنات", "https://images.unsplash.com/photo-1579372786545-d24232daf58c", 2),
                ("asian", "مطاعم آسيوية", "مطاعم الأكل الآسيوي", "https://images.unsplash.com/photo-1552566626-52f8b828add9", 3)
            };

            foreach (var (key, name, description, imageUrl, sortOrder) in desiredCategories)
            {
                // Check if a category with the same name already exists (inserted by another seeder)
                var existingCategory = await _categoryRepository.FirstOrDefaultAsync(c => c.Name == name);

                if (existingCategory != null)
                {
                    // Category already exists – reuse its Id
                    _categoryIds[key] = existingCategory.Id;
                    continue;
                }

                // Create and insert new category
                var categoryId = _guidGenerator.Create();
                _categoryIds[key] = categoryId;

                var newCategory = new RestaurantCategory(categoryId)
                {
                    Name = name,
                    Description = description,
                    ImageUrl = imageUrl,
                    IsActive = true,
                    SortOrder = sortOrder
                };

                await _categoryRepository.InsertAsync(newCategory);
            }
        }

        private async Task CreateSampleRestaurantsAsync()
        {
            // Restaurant 1: Fast Food
            var restaurant1Id = _guidGenerator.Create();
            _restaurantIds["restaurant1"] = restaurant1Id;
            
            var restaurant1 = new Restaurant(restaurant1Id)
            {
                Name = "مطعم العمدة",
                ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1074&q=80",
                CategoryId = _categoryIds["fast_food"],
                Rating = 4.5,
                DeliveryTime = "30-45 دقيقة",
                DeliveryFee = 5.00m,
                Description = "مطعم شاورما وكريسبي مميز",
                OwnerId = _userIds["restaurant_owner"], // Assign to restaurant owner
                Tags = new List<string> { "شاورما", "كريسبي", "وجبات سريعة" },
                Address = new Address(_guidGenerator.Create())
                {
                    Street = "شارع الرئيسي",
                    City = "دمشق",
                    State = "دمشق",
                    ZipCode = "10001",
                    FullAddress = "شارع الرئيسي، دمشق",
                    Latitude = 33.5138,
                    Longitude = 36.2765
                }
            };

            // Add menu items to restaurant 1
            var menuItem1Id = _guidGenerator.Create();
            _menuItemIds["menuItem1"] = menuItem1Id;
            
            var menuItem2Id = _guidGenerator.Create();
            _menuItemIds["menuItem2"] = menuItem2Id;
            
            var menuItem3Id = _guidGenerator.Create();
            _menuItemIds["menuItem3"] = menuItem3Id;
            
            restaurant1.Menu = new List<MenuItem>
            {
                new MenuItem(menuItem1Id)
                {
                    Name = "شاورما دجاج كبير",
                    Description = "شاورما دجاج كبير مع صوص خاص",
                    Price = 20.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1561651823-34feb02250e4?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1074&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    IsPopular = true,
                    Options = new List<string> { "حار", "متوسط", "عادي" },
                    RestaurantId = restaurant1Id
                },
                new MenuItem(menuItem2Id)
                {
                    Name = "شاورما لحم كبير",
                    Description = "شاورما لحم كبير مع صوص خاص",
                    Price = 25.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1561651823-34feb02250e4?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1074&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "حار", "متوسط", "عادي" },
                    RestaurantId = restaurant1Id
                },
                new MenuItem(menuItem3Id)
                {
                    Name = "كريسبي دجاج",
                    Description = "كريسبي دجاج مقرمش",
                    Price = 18.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "حار", "عادي" },
                    RestaurantId = restaurant1Id
                }
            };

            // Restaurant 2: Pizza
            var restaurant2Id = _guidGenerator.Create();
            _restaurantIds["restaurant2"] = restaurant2Id;
            
            var restaurant2 = new Restaurant(restaurant2Id)
            {
                Name = "بيتزا الريف",
                ImageUrl = "https://images.unsplash.com/photo-1590947132387-155cc02f3212?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                CategoryId = _categoryIds["fast_food"],
                Rating = 4.2,
                DeliveryTime = "40-55 دقيقة",
                DeliveryFee = 7.50m,
                Description = "بيتزا طازجة بأنواعها",
                OwnerId = _userIds["restaurant_owner"], // Assign to restaurant owner
                Tags = new List<string> { "بيتزا", "معجنات", "وجبات سريعة" },
                Address = new Address(_guidGenerator.Create())
                {
                    Street = "شارع الجلاء",
                    City = "دمشق",
                    State = "دمشق",
                    ZipCode = "10002",
                    FullAddress = "شارع الجلاء، دمشق",
                    Latitude = 33.5224,
                    Longitude = 36.2922
                }
            };

            // Add menu items to restaurant 2
            var menuItem4Id = _guidGenerator.Create();
            _menuItemIds["menuItem4"] = menuItem4Id;
            
            var menuItem5Id = _guidGenerator.Create();
            _menuItemIds["menuItem5"] = menuItem5Id;
            
            var menuItem6Id = _guidGenerator.Create();
            _menuItemIds["menuItem6"] = menuItem6Id;
            
            restaurant2.Menu = new List<MenuItem>
            {
                new MenuItem(menuItem4Id)
                {
                    Name = "بيتزا مارغريتا",
                    Description = "بيتزا مارغريتا مع جبنة موزاريلا وصلصة طماطم",
                    Price = 30.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1604917877934-07d8d248d396?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    IsPopular = true,
                    Options = new List<string> { "صغير", "وسط", "كبير" },
                    RestaurantId = restaurant2Id
                },
                new MenuItem(menuItem5Id)
                {
                    Name = "بيتزا خضار",
                    Description = "بيتزا خضار مع فلفل وطماطم وزيتون",
                    Price = 35.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1595708684082-a173bb3a06c5?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "صغير", "وسط", "كبير" },
                    RestaurantId = restaurant2Id
                },
                new MenuItem(menuItem6Id)
                {
                    Name = "بيتزا دجاج",
                    Description = "بيتزا دجاج مع فطر وجبنة",
                    Price = 40.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1081&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "صغير", "وسط", "كبير" },
                    RestaurantId = restaurant2Id
                }
            };

            // Restaurant 3: Sweets
            var restaurant3Id = _guidGenerator.Create();
            _restaurantIds["restaurant3"] = restaurant3Id;
            
            var restaurant3 = new Restaurant(restaurant3Id)
            {
                Name = "حلويات دمشقية",
                ImageUrl = "https://images.unsplash.com/photo-1579372786545-d24232daf58c?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                CategoryId = _categoryIds["sweets"],
                Rating = 4.8,
                DeliveryTime = "20-35 دقيقة",
                DeliveryFee = 6.00m,
                Description = "حلويات شرقية تقليدية",
                OwnerId = _userIds["restaurant_owner"], // Assign to restaurant owner
                Tags = new List<string> { "حلويات", "بقلاوة", "كنافة" },
                Address = new Address(_guidGenerator.Create())
                {
                    Street = "شارع الحمراء",
                    City = "دمشق",
                    State = "دمشق",
                    ZipCode = "10003",
                    FullAddress = "شارع الحمراء، دمشق",
                    Latitude = 33.5138,
                    Longitude = 36.2765
                }
            };

            // Add menu items to restaurant 3
            var menuItem7Id = _guidGenerator.Create();
            _menuItemIds["menuItem7"] = menuItem7Id;
            
            var menuItem8Id = _guidGenerator.Create();
            _menuItemIds["menuItem8"] = menuItem8Id;
            
            var menuItem9Id = _guidGenerator.Create();
            _menuItemIds["menuItem9"] = menuItem9Id;
            
            restaurant3.Menu = new List<MenuItem>
            {
                new MenuItem(menuItem7Id)
                {
                    Name = "بقلاوة",
                    Description = "بقلاوة بالفستق الحلبي",
                    Price = 15.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1625517236224-4ab3dd4f2da5?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    IsPopular = true,
                    Options = new List<string> { "كيلو", "نصف كيلو", "ربع كيلو" },
                    RestaurantId = restaurant3Id
                },
                new MenuItem(menuItem8Id)
                {
                    Name = "كنافة",
                    Description = "كنافة بالجبنة والقطر",
                    Price = 20.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1566133919355-d72f16e0d1c5?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "كيلو", "نصف كيلو", "ربع كيلو" },
                    RestaurantId = restaurant3Id
                },
                new MenuItem(menuItem9Id)
                {
                    Name = "حلاوة الجبن",
                    Description = "حلاوة الجبن بالقشطة",
                    Price = 18.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1541658016709-82535e94bc69?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1169&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "كيلو", "نصف كيلو", "ربع كيلو" },
                    RestaurantId = restaurant3Id
                }
            };
            
            // Restaurant 4: Asian Cuisine
            var restaurant4Id = _guidGenerator.Create();
            _restaurantIds["restaurant4"] = restaurant4Id;

            var restaurant4 = new Restaurant(restaurant4Id)
            {
                Name = "مطعم آسيا",
                ImageUrl = "https://images.unsplash.com/photo-1552566626-52f8b828add9?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                CategoryId = _categoryIds["asian"],
                Rating = 4.6,
                DeliveryTime = "35-50 دقيقة",
                DeliveryFee = 8.00m,
                Description = "أطباق آسيوية أصيلة",
                OwnerId = _userIds["restaurant_owner"],
                Tags = new List<string> { "صيني", "تايلندي", "سوشي" },
                Address = new Address(_guidGenerator.Create())
                {
                    Street = "شارع أبو رمانة",
                    City = "دمشق",
                    State = "دمشق",
                    ZipCode = "10004",
                    FullAddress = "شارع أبو رمانة، دمشق",
                    Latitude = 33.5170,
                    Longitude = 36.2810
                }
            };
            
            // Add menu items to restaurant 4
            var menuItem10Id = _guidGenerator.Create();
            _menuItemIds["menuItem10"] = menuItem10Id;
            
            var menuItem11Id = _guidGenerator.Create();
            _menuItemIds["menuItem11"] = menuItem11Id;
            
            var menuItem12Id = _guidGenerator.Create();
            _menuItemIds["menuItem12"] = menuItem12Id;
            
            restaurant4.Menu = new List<MenuItem>
            {
                new MenuItem(menuItem10Id)
                {
                    Name = "سوشي سلمون",
                    Description = "سوشي سلمون طازج مع صلصة الصويا",
                    Price = 45.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    IsPopular = true,
                    Options = new List<string> { "8 قطع", "16 قطعة" },
                    RestaurantId = restaurant4Id
                },
                new MenuItem(menuItem11Id)
                {
                    Name = "أرز مقلي بالخضار",
                    Description = "أرز مقلي مع خضار مشكلة وصلصة الصويا",
                    Price = 25.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1603133872878-684f208fb84b?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1025&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "حار", "عادي" },
                    RestaurantId = restaurant4Id
                },
                new MenuItem(menuItem12Id)
                {
                    Name = "نودلز بالدجاج",
                    Description = "نودلز مع دجاج وخضار مقلية بالصلصة التايلندية",
                    Price = 30.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1585032226651-759b368d7246?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1074&q=80",
                    MealCategoryId = null, // TODO: Assign proper meal category
                    IsAvailable = true,
                    Options = new List<string> { "حار", "متوسط", "عادي" },
                    RestaurantId = restaurant4Id
                }
            };

            // Save restaurants to database
            await _restaurantRepository.InsertAsync(restaurant1);
            await _restaurantRepository.InsertAsync(restaurant2);
            await _restaurantRepository.InsertAsync(restaurant3);
            await _restaurantRepository.InsertAsync(restaurant4);

            // Add favorite restaurants for customer
            if (_userIds.ContainsKey("customer"))
            {
                var appUser = await _userRepository.GetAsync(_userIds["customer"]);
                appUser.FavoriteRestaurants = new List<FavoriteRestaurant>
                {
                    new FavoriteRestaurant(_guidGenerator.Create())
                    {
                        UserId = _userIds["customer"],
                        RestaurantId = _restaurantIds["restaurant1"]
                    },
                    new FavoriteRestaurant(_guidGenerator.Create())
                    {
                        UserId = _userIds["customer"],
                        RestaurantId = _restaurantIds["restaurant3"]
                    }
                };
                await _userRepository.UpdateAsync(appUser);
            }
            
            // Add favorite restaurants for second customer
            if (_userIds.ContainsKey("customer2"))
            {
                var appUser = await _userRepository.GetAsync(_userIds["customer2"]);
                appUser.FavoriteRestaurants = new List<FavoriteRestaurant>
                {
                    new FavoriteRestaurant(_guidGenerator.Create())
                    {
                        UserId = _userIds["customer2"],
                        RestaurantId = _restaurantIds["restaurant2"]
                    },
                    new FavoriteRestaurant(_guidGenerator.Create())
                    {
                        UserId = _userIds["customer2"],
                        RestaurantId = _restaurantIds["restaurant4"]
                    }
                };
                await _userRepository.UpdateAsync(appUser);
            }
        }

        private OrderItem CreateOrderItem(Guid menuItemId, string name, int quantity, decimal price, 
            List<string> options, List<string> selectedOptions, string specialInstructions = "")
        {
            var orderItem = new OrderItem(_guidGenerator.Create())
            {
                MenuItemId = menuItemId,
                Name = name,
                Quantity = quantity,
                Price = price,
                Options = options,
                SelectedOptions = selectedOptions,
                SpecialInstructions = specialInstructions
            };
            return orderItem;
        }

        private async Task CreateSampleOrdersAsync()
        {
            // Create a completed order
            var completedOrderId = _guidGenerator.Create();
            _orderIds["completedOrder"] = completedOrderId;
            
            var completedOrder = new Order(completedOrderId)
            {
                RestaurantId = _restaurantIds["restaurant1"],
                UserId = _userIds["customer"],
                OrderDate = DateTime.Now.AddDays(-1),
                Subtotal = 58.00m,
                DeliveryFee = 5.00m,
                Tax = 4.64m,
                TotalAmount = 67.64m,
                EstimatedDeliveryTime = 30,
                Status = OrderStatus.Delivered,
                DeliveryAddressId = _addressIds["home"],
                PaymentMethodId = _paymentMethodIds["credit"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem1"],
                        "شاورما دجاج كبير",
                        2,
                        20.00m,
                        new List<string> { "حار", "متوسط", "عادي" },
                        new List<string> { "حار" },
                        "بدون بصل"
                    ),
                    CreateOrderItem(
                        _menuItemIds["menuItem3"],
                        "كريسبي دجاج",
                        1,
                        18.00m,
                        new List<string> { "حار", "عادي" },
                        new List<string> { "حار" }
                    )
                }
            };

            // Create an in-delivery order
            var inDeliveryOrderId = _guidGenerator.Create();
            _orderIds["inDeliveryOrder"] = inDeliveryOrderId;
            
            var inDeliveryOrder = new Order(inDeliveryOrderId)
            {
                RestaurantId = _restaurantIds["restaurant2"],
                UserId = _userIds["customer"],
                DeliveryPersonId = _userIds["delivery"], // Assign to delivery person
                OrderDate = DateTime.Now.AddHours(-1),
                Subtotal = 70.00m,
                DeliveryFee = 7.50m,
                Tax = 5.60m,
                TotalAmount = 83.10m,
                EstimatedDeliveryTime = 45,
                Status = OrderStatus.Delivering,
                DeliveryAddressId = _addressIds["home"],
                PaymentMethodId = _paymentMethodIds["cash"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem4"],
                        "بيتزا مارغريتا",
                        1,
                        30.00m,
                        new List<string> { "صغير", "وسط", "كبير" },
                        new List<string> { "وسط" }
                    ),
                    CreateOrderItem(
                        _menuItemIds["menuItem5"],
                        "بيتزا خضار",
                        1,
                        35.00m,
                        new List<string> { "صغير", "وسط", "كبير" },
                        new List<string> { "كبير" },
                        "إضافة جبنة"
                    )
                }
            };

            // Create a pending order
            var pendingOrderId = _guidGenerator.Create();
            _orderIds["pendingOrder"] = pendingOrderId;
            
            var pendingOrder = new Order(pendingOrderId)
            {
                RestaurantId = _restaurantIds["restaurant3"],
                UserId = _userIds["customer"],
                OrderDate = DateTime.Now.AddMinutes(-15),
                Subtotal = 34.00m,
                DeliveryFee = 6.00m,
                Tax = 0.80m,
                TotalAmount = 40.80m,
                EstimatedDeliveryTime = 0, // Not yet assigned
                Status = OrderStatus.WaitingCourier,
                DeliveryAddressId = _addressIds["work"],
                PaymentMethodId = _paymentMethodIds["credit"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem7"],
                        "بقلاوة",
                        1,
                        15.00m,
                        new List<string> { "كيلو", "نصف كيلو", "ربع كيلو" },
                        new List<string> { "نصف كيلو" }
                    ),
                    CreateOrderItem(
                        _menuItemIds["menuItem9"],
                        "حلاوة الجبن",
                        1,
                        18.00m,
                        new List<string> { "كيلو", "نصف كيلو", "ربع كيلو" },
                        new List<string> { "ربع كيلو" }
                    )
                }
            };

            // Create a ready for delivery order
            var readyForDeliveryOrderId = _guidGenerator.Create();
            _orderIds["readyForDeliveryOrder"] = readyForDeliveryOrderId;
            
            var readyForDeliveryOrder = new Order(readyForDeliveryOrderId)
            {
                RestaurantId = _restaurantIds["restaurant1"],
                UserId = _userIds["customer"],
                OrderDate = DateTime.Now.AddMinutes(-30),
                Subtotal = 45.00m,
                DeliveryFee = 5.00m,
                Tax = 3.60m,
                TotalAmount = 53.60m,
                EstimatedDeliveryTime = 30,
                Status = OrderStatus.ReadyForDelivery,
                DeliveryAddressId = _addressIds["home"],
                PaymentMethodId = _paymentMethodIds["cash"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem1"],
                        "شاورما دجاج كبير",
                        1,
                        20.00m,
                        new List<string> { "حار", "متوسط", "عادي" },
                        new List<string> { "متوسط" }
                    ),
                    CreateOrderItem(
                        _menuItemIds["menuItem2"],
                        "شاورما لحم كبير",
                        1,
                        25.00m,
                        new List<string> { "حار", "متوسط", "عادي" },
                        new List<string> { "حار" }
                    )
                }
            };

            // Create an order for restaurant owner to manage
            var restaurantOwnerOrderId = _guidGenerator.Create();
            _orderIds["restaurantOwnerOrder"] = restaurantOwnerOrderId;
            
            var restaurantOwnerOrder = new Order(restaurantOwnerOrderId)
            {
                RestaurantId = _restaurantIds["restaurant1"],
                UserId = _userIds["customer"],
                OrderDate = DateTime.Now.AddMinutes(-5),
                Subtotal = 38.00m,
                DeliveryFee = 5.00m,
                Tax = 3.04m,
                TotalAmount = 46.04m,
                EstimatedDeliveryTime = 0, // Not yet assigned
                Status = OrderStatus.Preparing, // Being prepared
                DeliveryAddressId = _addressIds["home"],
                PaymentMethodId = _paymentMethodIds["credit"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem1"],
                        "شاورما دجاج كبير",
                        1,
                        20.00m,
                        new List<string> { "حار", "متوسط", "عادي" },
                        new List<string> { "عادي" }
                    ),
                    CreateOrderItem(
                        _menuItemIds["menuItem3"],
                        "كريسبي دجاج",
                        1,
                        18.00m,
                        new List<string> { "حار", "عادي" },
                        new List<string> { "عادي" }
                    )
                }
            };
            
            // Create older orders for order history
            var olderOrder1Id = _guidGenerator.Create();
            _orderIds["olderOrder1"] = olderOrder1Id;
            
            var olderOrder1 = new Order(olderOrder1Id)
            {
                RestaurantId = _restaurantIds["restaurant1"],
                UserId = _userIds["customer"],
                OrderDate = DateTime.Now.AddDays(-7),
                Subtotal = 40.00m,
                DeliveryFee = 5.00m,
                Tax = 3.20m,
                TotalAmount = 48.20m,
                EstimatedDeliveryTime = 30,
                Status = OrderStatus.Delivered,
                DeliveryAddressId = _addressIds["home"],
                PaymentMethodId = _paymentMethodIds["credit"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem1"],
                        "شاورما دجاج كبير",
                        2,
                        20.00m,
                        new List<string> { "حار", "متوسط", "عادي" },
                        new List<string> { "متوسط" }
                    )
                }
            };
            
            var olderOrder2Id = _guidGenerator.Create();
            _orderIds["olderOrder2"] = olderOrder2Id;
            
            var olderOrder2 = new Order(olderOrder2Id)
            {
                RestaurantId = _restaurantIds["restaurant2"],
                UserId = _userIds["customer"],
                OrderDate = DateTime.Now.AddDays(-14),
                Subtotal = 65.00m,
                DeliveryFee = 7.50m,
                Tax = 5.20m,
                TotalAmount = 77.70m,
                EstimatedDeliveryTime = 45,
                Status = OrderStatus.Delivered,
                DeliveryAddressId = _addressIds["work"],
                PaymentMethodId = _paymentMethodIds["cash"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem4"],
                        "بيتزا مارغريتا",
                        1,
                        30.00m,
                        new List<string> { "صغير", "وسط", "كبير" },
                        new List<string> { "كبير" }
                    ),
                    CreateOrderItem(
                        _menuItemIds["menuItem6"],
                        "بيتزا دجاج",
                        1,
                        35.00m,
                        new List<string> { "صغير", "وسط", "كبير" },
                        new List<string> { "وسط" }
                    )
                }
            };
            
            // Create orders for second customer
            var customer2OrderId = _guidGenerator.Create();
            _orderIds["customer2Order"] = customer2OrderId;
            
            var customer2Order = new Order(customer2OrderId)
            {
                RestaurantId = _restaurantIds["restaurant4"],
                UserId = _userIds["customer2"],
                OrderDate = DateTime.Now.AddHours(-2),
                Subtotal = 75.00m,
                DeliveryFee = 8.00m,
                Tax = 6.00m,
                TotalAmount = 89.00m,
                EstimatedDeliveryTime = 40,
                Status = OrderStatus.Delivered,
                DeliveryAddressId = _addressIds["home2"],
                PaymentMethodId = _paymentMethodIds["credit2"],
                Items = new List<OrderItem>
                {
                    CreateOrderItem(
                        _menuItemIds["menuItem10"],
                        "سوشي سلمون",
                        1,
                        45.00m,
                        new List<string> { "8 قطع", "16 قطعة" },
                        new List<string> { "8 قطع" }
                    ),
                    CreateOrderItem(
                        _menuItemIds["menuItem11"],
                        "أرز مقلي بالخضار",
                        1,
                        25.00m,
                        new List<string> { "حار", "عادي" },
                        new List<string> { "حار" }
                    )
                }
            };

            // Save orders to database
            await _orderRepository.InsertAsync(completedOrder);
            await _orderRepository.InsertAsync(inDeliveryOrder);
            await _orderRepository.InsertAsync(pendingOrder);
            await _orderRepository.InsertAsync(readyForDeliveryOrder);
            await _orderRepository.InsertAsync(restaurantOwnerOrder);
            await _orderRepository.InsertAsync(olderOrder1);
            await _orderRepository.InsertAsync(olderOrder2);
            await _orderRepository.InsertAsync(customer2Order);
            
            // Update delivery person status with current order
            if (_userIds.ContainsKey("delivery"))
            {
                var deliveryUser = await _userRepository.GetAsync(_userIds["delivery"], includeDetails: true);

                if (deliveryUser.DeliveryStatus == null)
                {
                    // First time seeding delivery status for this user
                    deliveryUser.DeliveryStatus = new DeliveryStatus(deliveryUser.Id)
                    {
                        IsAvailable = true,
                        CurrentOrderId = _orderIds["inDeliveryOrder"],
                        LastStatusUpdate = DateTime.Now
                    };
                }
                else
                {
                    // Update existing status to reference the newly created order
                    deliveryUser.DeliveryStatus.IsAvailable = true;
                    deliveryUser.DeliveryStatus.CurrentOrderId = _orderIds["inDeliveryOrder"];
                    deliveryUser.DeliveryStatus.LastStatusUpdate = DateTime.Now;
                }

                await _userRepository.UpdateAsync(deliveryUser);
            }
        }
        
        private async Task CreateSampleReviewsAsync()
        {
            var reviews = new List<Review>
            {
                new Review(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant1"],
                    UserId = _userIds["customer"],
                    Rating = 5,
                    Comment = "طعام رائع وخدمة سريعة",
                    Date = DateTime.Now.AddDays(-3)
                },
                new Review(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant2"],
                    UserId = _userIds["customer"],
                    Rating = 4,
                    Comment = "البيتزا لذيذة لكن التوصيل تأخر قليلاً",
                    Date = DateTime.Now.AddDays(-10)
                },
                new Review(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant3"],
                    UserId = _userIds["customer"],
                    Rating = 5,
                    Comment = "الحلويات طازجة ولذيذة جداً",
                    Date = DateTime.Now.AddDays(-5)
                },
                new Review(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant4"],
                    UserId = _userIds["customer2"],
                    Rating = 5,
                    Comment = "أفضل سوشي في المدينة",
                    Date = DateTime.Now.AddDays(-2)
                },
                new Review(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant2"],
                    UserId = _userIds["customer2"],
                    Rating = 3,
                    Comment = "البيتزا جيدة ولكن يمكن أن تكون أفضل",
                    Date = DateTime.Now.AddDays(-7)
                }
            };
            
            await _reviewRepository.InsertManyAsync(reviews);
        }
        
        private async Task CreateSpecialOffersAsync()
        {
            var offers = new List<SpecialOffer>
            {
                new SpecialOffer(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant1"],
                    Title = "خصم 20% على الطلبات فوق 100 ليرة",
                    Description = "استمتع بخصم 20% على جميع الطلبات التي تزيد قيمتها عن 100 ليرة",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(7),
                    DiscountPercentage = 20,
                    MinimumOrderAmount = 100
                },
                new SpecialOffer(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant2"],
                    Title = "توصيل مجاني",
                    Description = "استمتع بتوصيل مجاني لجميع الطلبات هذا الأسبوع",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(5),
                    FreeDelivery = true
                },
                new SpecialOffer(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant3"],
                    Title = "اشتري واحدة واحصل على الثانية مجاناً",
                    Description = "عند شراء كيلو من الحلويات احصل على نصف كيلو مجاناً",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(3),
                    BuyOneGetOne = true
                },
                new SpecialOffer(_guidGenerator.Create())
                {
                    RestaurantId = _restaurantIds["restaurant4"],
                    Title = "خصم 15% على أطباق السوشي",
                    Description = "استمتع بخصم 15% على جميع أطباق السوشي",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(10),
                    DiscountPercentage = 15,
                    ApplicableCategories = new List<string> { "سوشي" }
                }
            };
            
            await _offerRepository.InsertManyAsync(offers);
        }

        private async Task CreateSampleAdvertisementsAsync()
        {
            var advertisements = new List<Advertisement>
            {
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "توصيل مجاني للطلبات أكثر من 50 ريال",
                    Description = "احصل على توصيل مجاني لجميع الطلبات التي تزيد عن 50 ريال سعودي",
                    ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ca4b?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants",
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(30),
                    IsActive = true,
                    Priority = 1,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "عرض خاص: خصم 20% على البيتزا",
                    Description = "استمتع بخصم 20% على جميع أنواع البيتزا من مطاعمنا المختارة",
                    ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants?category=pizza",
                    StartDate = DateTime.Now.AddDays(-2),
                    EndDate = DateTime.Now.AddDays(15),
                    IsActive = true,
                    Priority = 2,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "مطعم الشرق الأوسط - أطباق عربية أصيلة",
                    Description = "تذوق ألذ الأطباق العربية الأصيلة من مطعم الشرق الأوسط",
                    ImageUrl = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/middle-east-restaurant",
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(45),
                    IsActive = true,
                    Priority = 3,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "وجبات صحية من مطعم الصحة واللياقة",
                    Description = "احصل على وجبات صحية ومتوازنة من مطعم الصحة واللياقة",
                    ImageUrl = "https://images.unsplash.com/photo-1490645935967-10de6ba17061?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/health-fitness-restaurant",
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now.AddDays(20),
                    IsActive = true,
                    Priority = 4,
                    TargetAudience = "Health Conscious",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "سوشي طازج من اليابان",
                    Description = "استمتع بأفضل أنواع السوشي الطازج من مطعم السوشي الياباني",
                    ImageUrl = "https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/japanese-sushi-restaurant",
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(25),
                    IsActive = true,
                    Priority = 5,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                },
                new Advertisement(_guidGenerator.Create())
                {
                    Title = "برجر لحم بقري طازج",
                    Description = "تذوق أفضل برجر لحم بقري طازج من مطعم البرجر الأمريكي",
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=800&h=400&fit=crop",
                    LinkUrl = "/restaurants/american-burger-restaurant",
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(18),
                    IsActive = true,
                    Priority = 6,
                    TargetAudience = "All",
                    Location = "All",
                    ClickCount = 0,
                    ViewCount = 0
                }
            };
            
            await _advertisementRepository.InsertManyAsync(advertisements);
        }
    }
}
