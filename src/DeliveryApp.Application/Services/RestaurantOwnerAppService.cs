using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace DeliveryApp.Application.Services
{
    [Authorize(Roles = "restaurant_owner")]
    public class RestaurantOwnerAppService : ApplicationService, IRestaurantOwnerAppService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IRepository<MenuItem, Guid> _menuItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IdentityUserManager _userManager;
        private readonly ICurrentUser _currentUser;
        private readonly IOrderStatusNotificationService _orderStatusNotifier;
        private readonly IDeliveryAssignmentService _deliveryAssignmentService;
        private readonly IRestaurantReportService _restaurantReportService;
        private readonly IMealCategoryRepository _mealCategoryRepository;
        private readonly ISpecialOfferRepository _specialOfferRepository;
        private readonly IAdRequestRepository _adRequestRepository;

        public RestaurantOwnerAppService(
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository,
            IRepository<MenuItem, Guid> menuItemRepository,
            IOrderRepository orderRepository,
            IdentityUserManager userManager,
            ICurrentUser currentUser,
            IOrderStatusNotificationService orderStatusNotifier,
            IDeliveryAssignmentService deliveryAssignmentService,
            IRestaurantReportService restaurantReportService,
            IMealCategoryRepository mealCategoryRepository,
            ISpecialOfferRepository specialOfferRepository,
            IAdRequestRepository adRequestRepository)
        {
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
            _menuItemRepository = menuItemRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _currentUser = currentUser;
            _orderStatusNotifier = orderStatusNotifier;
            _deliveryAssignmentService = deliveryAssignmentService;
            _restaurantReportService = restaurantReportService;
            _mealCategoryRepository = mealCategoryRepository;
            _specialOfferRepository = specialOfferRepository;
            _adRequestRepository = adRequestRepository;
        }

        private Guid GetCurrentUserId()
        {
            // Try to get user ID from current user context first
            if (_currentUser.IsAuthenticated)
            {
                var userId = _currentUser.GetId();
                if (userId != null)
                {
                    return userId;
                }
            }
            
            // If current user context doesn't work, try to get from JWT claims
            // This is a fallback for when the current user context is not properly set
            throw new UnauthorizedAccessException("User ID not found. Please ensure you are properly authenticated.");
        }

        public async Task<RestaurantOwnerDto> GetProfileAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var user = await _userRepository.GetUserWithDetailsAsync(userGuid);
            
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userGuid} not found");
            }
            
            var restaurantOwnerDto = ObjectMapper.Map<AppUser, RestaurantOwnerDto>(user);
            
            // Get managed restaurants
            var restaurants = await _restaurantRepository.GetListWithCategoryAsync(r => r.OwnerId == userGuid);
            restaurantOwnerDto.ManagedRestaurantIds = restaurants.Select(r => r.Id).ToList();
            restaurantOwnerDto.ManagedRestaurants = ObjectMapper.Map<List<Restaurant>, List<RestaurantSummaryDto>>(restaurants);
            
            return restaurantOwnerDto;
        }

        public async Task<RestaurantOwnerDto> UpdateProfileAsync(UpdateUserDto input)
        {
            if (!Guid.TryParse(input.UserId, out var userId))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            
            user.Name = input.Name;
            user.SetPhoneNumber(input.PhoneNumber, false);
            user.ProfileImageUrl = input.ProfileImageUrl;
            
            await _userRepository.UpdateAsync(user);
            
            return await GetProfileAsync(input.UserId);
        }

        public async Task<List<RestaurantDto>> GetManagedRestaurantsAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurants = await _restaurantRepository.GetListWithCategoryAsync(r => r.OwnerId == userGuid);
            
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        public async Task<RestaurantDto> GetRestaurantDetailsAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetWithCategoryAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> CreateRestaurantAsync(CreateRestaurantDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurantId = GuidGenerator.Create();
            var restaurant = new Restaurant(restaurantId)
            {
                OwnerId = userGuid,
                Name = input.Name,
                Description = input.Description,
                CategoryId = input.CategoryId,
                ImageUrl = input.ImageUrl,
                DeliveryFee = input.DeliveryFee,
                MinimumOrderAmount = input.MinimumOrderAmount,
                Rating = 0, // New restaurant starts with no rating
                Address = new Address(GuidGenerator.Create())
                {
                    Street = input.Address.Street,
                    City = input.Address.City,
                    State = input.Address.State,
                    ZipCode = input.Address.ZipCode,
                    FullAddress = input.Address.FullAddress,
                    Latitude = input.Address.Latitude,
                    Longitude = input.Address.Longitude
                }
            };
            
            // Add tags
            if (input.Tags != null)
            {
                foreach (var tag in input.Tags)
                {
                    restaurant.Tags.Add(tag);
                }
            }
            
            await _restaurantRepository.InsertAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> UpdateRestaurantAsync(Guid restaurantId, UpdateRestaurantDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            // Update restaurant properties
            restaurant.Name = input.Name;
            restaurant.Description = input.Description;
            restaurant.CategoryId = input.CategoryId;
            restaurant.ImageUrl = input.ImageUrl;
            restaurant.DeliveryFee = input.DeliveryFee;
            restaurant.MinimumOrderAmount = input.MinimumOrderAmount;
            
            // Update address
            if (input.Address != null)
            {
                restaurant.Address.Street = input.Address.Street;
                restaurant.Address.City = input.Address.City;
                restaurant.Address.State = input.Address.State;
                restaurant.Address.ZipCode = input.Address.ZipCode;
                restaurant.Address.FullAddress = input.Address.FullAddress;
                restaurant.Address.Latitude = input.Address.Latitude;
                restaurant.Address.Longitude = input.Address.Longitude;
            }
            
            // Update tags
            restaurant.Tags.Clear();
            if (input.Tags != null)
            {
                foreach (var tag in input.Tags)
                {
                    restaurant.Tags.Add(tag);
                }
            }
            
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<List<MenuItemDto>> GetMenuItemsAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var menuItems = await _menuItemRepository.GetListAsync(m => m.RestaurantId == restaurantId);
            
            return ObjectMapper.Map<List<MenuItem>, List<MenuItemDto>>(menuItems);
        }

        public async Task<MenuItemDto> AddMenuItemAsync(Guid restaurantId, CreateMenuItemDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var menuItemId = GuidGenerator.Create();
            var menuItem = new MenuItem(menuItemId)
            {
                RestaurantId = restaurantId,
                Name = input.Name,
                Description = input.Description,
                Price = input.Price,
                MealCategoryId = input.MealCategoryId,
                ImageUrl = input.ImageUrl ?? string.Empty,
                IsAvailable = input.IsAvailable,
                PreparationMinutes = input.PreparationMinutes
            };
            
            // Add options
            if (input.Options != null)
            {
                foreach (var option in input.Options)
                {
                    menuItem.Options.Add(option);
                }
            }
            
            await _menuItemRepository.InsertAsync(menuItem);
            
            return ObjectMapper.Map<MenuItem, MenuItemDto>(menuItem);
        }

        public async Task<MenuItemDto> UpdateMenuItemAsync(Guid restaurantId, Guid menuItemId, UpdateMenuItemDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var menuItem = await _menuItemRepository.GetAsync(m => m.Id == menuItemId && m.RestaurantId == restaurantId);
            
            // Update menu item properties
            menuItem.Name = input.Name;
            menuItem.Description = input.Description;
            menuItem.Price = input.Price;
            menuItem.MealCategoryId = input.MealCategoryId;
            menuItem.ImageUrl = input.ImageUrl;
            menuItem.IsAvailable = input.IsAvailable;
            menuItem.PreparationMinutes = input.PreparationMinutes;
            
            // Update options
            menuItem.Options.Clear();
            if (input.Options != null)
            {
                foreach (var option in input.Options)
                {
                    menuItem.Options.Add(option);
                }
            }
            
            await _menuItemRepository.UpdateAsync(menuItem);
            
            return ObjectMapper.Map<MenuItem, MenuItemDto>(menuItem);
        }

        public async Task<bool> DeleteMenuItemAsync(Guid restaurantId, Guid menuItemId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var menuItem = await _menuItemRepository.GetAsync(m => m.Id == menuItemId && m.RestaurantId == restaurantId);
            
            await _menuItemRepository.DeleteAsync(menuItem);
            
            return true;
        }

        public async Task<bool> UpdateMenuItemAvailabilityAsync(Guid restaurantId, Guid menuItemId, bool isAvailable, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var menuItem = await _menuItemRepository.GetAsync(menuItemId);
            
            // Verify the menu item belongs to this restaurant
            if (menuItem.RestaurantId != restaurantId)
            {
                throw new UnauthorizedAccessException("Menu item does not belong to this restaurant.");
            }
            
            menuItem.IsAvailable = isAvailable;
            await _menuItemRepository.UpdateAsync(menuItem);
            
            return true;
        }

        public async Task<List<string>> GetRestaurantCategoriesAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            // Get all menu items for this restaurant and extract unique categories
            var menuItems = await _menuItemRepository.GetListAsync(m => m.RestaurantId == restaurantId);
            // Get unique meal categories for this restaurant
            var categories = await _menuItemRepository.GetListAsync(m => m.RestaurantId == restaurantId && m.MealCategoryId != null);
            var categoryNames = categories.Where(m => m.MealCategory != null).Select(m => m.MealCategory!.Name).Distinct().ToList();
            
            // Return only real categories from database, no defaults
            return categoryNames;
        }

        public async Task<List<OrderDto>> GetRestaurantOrdersAsync(Guid restaurantId, OrderStatus? status, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var orders = await _orderRepository.GetRestaurantOrdersAsync(restaurantId, status, 0, 100);
            
            return ObjectMapper.Map<List<Order>, List<OrderDto>>(orders);
        }

        public async Task<OrderDto> GetOrderDetailsAsync(Guid orderId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            
            // Verify the order belongs to a restaurant owned by this user
            var restaurant = await _restaurantRepository.GetAsync(order.RestaurantId);
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this order's restaurant.");
            }
            
            return ObjectMapper.Map<Order, OrderDto>(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            
            // Verify the order belongs to a restaurant owned by this user
            var restaurant = await _restaurantRepository.GetAsync(order.RestaurantId);
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this order's restaurant.");
            }
            
            var previousStatus = order.Status;
            order.Status = status;
            await _orderRepository.UpdateAsync(order);
            
            // Auto-assign delivery person when order is ready for delivery
            if (status == OrderStatus.ReadyForDelivery)
            {
                try
                {
                    var assignmentResult = await _deliveryAssignmentService.AssignNearestDeliveryPersonAsync(orderId);
                    if (assignmentResult.Success)
                    {
                        // Reload order to get updated status after assignment
                        order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the status update
                    Logger.LogWarning($"Failed to auto-assign delivery person for order {orderId}. Error: {ex.Message}");
                }
            }
            
            // Send comprehensive notifications to all relevant parties
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(orderId, previousStatus, order.Status, userGuid);
            
            return ObjectMapper.Map<Order, OrderDto>(order);
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            
            // Verify the order belongs to a restaurant owned by this user
            var restaurant = await _restaurantRepository.GetAsync(order.RestaurantId);
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this order's restaurant.");
            }
            
            // Check if order can be cancelled based on current status
            if (order.Status == OrderStatus.Delivered)
            {
                throw new InvalidOperationException("Cannot cancel an order that has already been delivered.");
            }
            
            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("This order has already been cancelled.");
            }
            
            if (order.Status == OrderStatus.Delivering)
            {
                throw new InvalidOperationException("Cannot cancel an order that is currently being delivered. Please contact the delivery person directly.");
            }
            
            var previousStatus = order.Status;
            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
            
            // Send comprehensive notifications to all relevant parties
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(orderId, previousStatus, OrderStatus.Cancelled, userGuid);
            
            return true;
        }

        public async Task<RestaurantStatisticsDto> GetRestaurantStatisticsAsync(Guid restaurantId, DateTime? startDate, DateTime? endDate, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var statistics = new RestaurantStatisticsDto
            {
                TotalOrders = 0,
                TotalRevenue = 0,
                AverageOrderValue = 0,
                TopSellingItems = new List<MenuItemSalesDto>()
            };
            
            // Get orders for the specified date range
            var orders = await _orderRepository.GetRestaurantOrdersAsync(restaurantId, null, 0, 1000);
            
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.CreationTime >= startDate.Value).ToList();
            }
            
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.CreationTime <= endDate.Value).ToList();
            }
            
            // Calculate statistics
            statistics.TotalOrders = orders.Count;
            statistics.TotalRevenue = orders.Sum(o => o.TotalAmount);
            statistics.AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;
            
            // Get top selling items
            var topItems = await _menuItemRepository.GetListAsync(m => m.RestaurantId == restaurantId);
            
            // Convert to MenuItemSalesDto (simplified - in a real app you'd calculate actual sales)
            var topItemsSalesDto = topItems
                .OrderByDescending(x => x.Price)
                .Take(10)
                .Select(item => new MenuItemSalesDto
                {
                    MenuItemId = item.Id,
                    Name = item.Name,
                    QuantitySold = 1, // Placeholder - in real app this would be calculated from orders
                    Revenue = item.Price
                })
                .ToList();
            
            statistics.TopSellingItems = topItemsSalesDto;
            
            return statistics;
        }

        // Admin methods for restaurant approval
        [Authorize(Roles = "admin")]
        public async Task<List<RestaurantDto>> GetPendingApprovalRestaurantsAsync()
        {
            // Get restaurants that are waiting for admin approval
            var restaurants = await _restaurantRepository.GetListAsync(r => 
                r.Tags.Contains("في انتظار الموافقة") || 
                r.Name.Contains("مطعم جديد"));
            
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        [Authorize(Roles = "admin")]
        public async Task<RestaurantDto> ApproveRestaurantAsync(Guid restaurantId)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Update restaurant to approved status
            // Create a new list to ensure EF Core detects the change
            var newTags = restaurant.Tags.ToList();
            newTags.Remove("في انتظار الموافقة");
            newTags.Add("موافق عليه");
            restaurant.Tags = newTags;
            restaurant.Name = restaurant.Name.Replace("مطعم جديد", "مطعم معتمد");
            restaurant.Description = "مطعم معتمد من الإدارة";
            
            await _restaurantRepository.UpdateAsync(restaurant, autoSave: true);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        [Authorize(Roles = "admin")]
        public async Task<RestaurantDto> RejectRestaurantAsync(Guid restaurantId, string reason)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Update restaurant to rejected status
            // Create a new list to ensure EF Core detects the change
            var newTags = restaurant.Tags.ToList();
            newTags.Remove("في انتظار الموافقة");
            newTags.Add("مرفوض");
            newTags.Add($"سبب الرفض: {reason}");
            restaurant.Tags = newTags;
            
            await _restaurantRepository.UpdateAsync(restaurant, autoSave: true);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        // Restaurant owner method to check approval status
        public async Task<RestaurantApprovalStatusDto> GetRestaurantApprovalStatusAsync(Guid restaurantId)
        {
            var userId = GetCurrentUserId();
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var isPending = restaurant.Tags.Contains("في انتظار الموافقة");
            var isApproved = restaurant.Tags.Contains("موافق عليه");
            var isRejected = restaurant.Tags.Contains("مرفوض");
            
            string message;
            RestaurantApprovalStatus status;
            if (isPending)
            {
                status = RestaurantApprovalStatus.Pending;
                message = "مطعمك في انتظار مراجعة الإدارة";
            }
            else if (isApproved)
            {
                status = RestaurantApprovalStatus.Approved;
                message = "تمت الموافقة على مطعمك";
            }
            else if (isRejected)
            {
                status = RestaurantApprovalStatus.Rejected;
                var rejectionReason = restaurant.Tags.FirstOrDefault(t => t.StartsWith("سبب الرفض: "));
                message = rejectionReason ?? "تم رفض مطعمك";
            }
            else
            {
                status = RestaurantApprovalStatus.Unknown;
                message = "حالة غير معروفة";
            }
            
            return new RestaurantApprovalStatusDto
            {
                RestaurantId = restaurantId,
                Status = status,
                Message = message,
                LastUpdated = restaurant.LastModificationTime ?? restaurant.CreationTime
            };
        }

        #region Monthly Reports Implementation

        public async Task<MonthlyRestaurantReportDto> GetMonthlyReportAsync(Guid restaurantId, int year, int month, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            // Verify restaurant ownership
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            var request = new ReportRequestDto
            {
                RestaurantId = restaurantId,
                Year = year,
                Month = month,
                IncludeDetailedBreakdown = true,
                IncludePopularItems = true,
                IncludePerformanceMetrics = true
            };

            return await _restaurantReportService.GenerateMonthlyReportAsync(request);
        }

        public async Task<RestaurantPerformanceMetricsDto> GetPerformanceMetricsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            // Verify restaurant ownership
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            return await _restaurantReportService.GetPerformanceMetricsAsync(restaurantId, fromDate, toDate);
        }

        public async Task<List<PopularMenuItemDto>> GetTopSellingItemsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            // Verify restaurant ownership
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            return await _restaurantReportService.GetTopSellingItemsAsync(restaurantId, fromDate, toDate, 10);
        }

        public async Task<CommissionSummaryDto> GetCommissionSummaryAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            // Verify restaurant ownership
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            return await _restaurantReportService.GetCommissionSummaryAsync(restaurantId, fromDate, toDate);
        }

        #endregion

        #region Meal Category Management

        public async Task<List<MealCategoryDto>> GetMealCategoriesAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var categories = await _mealCategoryRepository.GetByRestaurantIdOrderedAsync(restaurantId);
            return ObjectMapper.Map<List<MealCategory>, List<MealCategoryDto>>(categories);
        }

        public async Task<MealCategoryDto> CreateMealCategoryAsync(Guid restaurantId, CreateMealCategoryDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Check if name is unique within restaurant
            if (!await _mealCategoryRepository.IsNameUniqueInRestaurantAsync(input.Name, restaurantId))
            {
                throw new ArgumentException($"A meal category with name '{input.Name}' already exists in this restaurant.");
            }

            var category = new MealCategory(GuidGenerator.Create())
            {
                RestaurantId = restaurantId,
                Name = input.Name,
                SortOrder = input.SortOrder
            };

            await _mealCategoryRepository.InsertAsync(category);
            return ObjectMapper.Map<MealCategory, MealCategoryDto>(category);
        }

        public async Task<MealCategoryDto> UpdateMealCategoryAsync(Guid categoryId, UpdateMealCategoryDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var category = await _mealCategoryRepository.GetAsync(categoryId);
            var restaurant = await _restaurantRepository.GetAsync(category.RestaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Check if name is unique within restaurant (excluding current category)
            if (!await _mealCategoryRepository.IsNameUniqueInRestaurantAsync(input.Name, category.RestaurantId, categoryId))
            {
                throw new ArgumentException($"A meal category with name '{input.Name}' already exists in this restaurant.");
            }

            category.Name = input.Name;
            category.SortOrder = input.SortOrder;

            await _mealCategoryRepository.UpdateAsync(category);
            return ObjectMapper.Map<MealCategory, MealCategoryDto>(category);
        }

        public async Task<bool> DeleteMealCategoryAsync(Guid categoryId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var category = await _mealCategoryRepository.GetAsync(categoryId);
            var restaurant = await _restaurantRepository.GetAsync(category.RestaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            await _mealCategoryRepository.DeleteAsync(category);
            return true;
        }

        #endregion

        #region Offers Management

        public async Task<List<SpecialOfferDto>> GetOffersAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var offers = await _specialOfferRepository.GetListAsync(0, 1000, "CreationTime", restaurantId);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        public async Task<SpecialOfferDto> CreateOfferAsync(Guid restaurantId, CreateSpecialOfferDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Set the restaurant ID from parameter to ensure consistency
            input.RestaurantId = restaurantId;
            
            var offer = new SpecialOffer(GuidGenerator.Create())
            {
                RestaurantId = restaurantId,
                Title = input.Title,
                Description = input.Description ?? string.Empty,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                IsRecurring = input.IsRecurring,
                RecurrencePattern = input.RecurrencePattern,
                MaxOccurrences = input.MaxOccurrences,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                ApplicableDays = input.ApplicableDays ?? new List<DayOfWeek>(),
                DiscountPercentage = input.DiscountPercentage,
                MinimumOrderAmount = input.MinimumOrderAmount,
                FreeDelivery = input.FreeDelivery,
                BuyOneGetOne = input.BuyOneGetOne,
                ApplicableCategories = input.ApplicableCategories ?? new List<string>(),
                Priority = input.Priority,
                MaxUses = input.MaxUses,
                IsActive = input.IsActive,
                Status = input.IsActive ? OfferStatus.Active : OfferStatus.Draft
            };
            
            if (offer.IsRecurring)
            {
                offer.NextOccurrence = offer.StartDate;
            }
            
            await _specialOfferRepository.InsertAsync(offer);
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        public async Task<SpecialOfferDto> UpdateOfferAsync(Guid offerId, UpdateSpecialOfferDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var offer = await _specialOfferRepository.GetAsync(offerId);
            var restaurant = await _restaurantRepository.GetAsync(offer.RestaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            ObjectMapper.Map(input, offer);
            
            if (offer.IsRecurring && offer.NextOccurrence == null)
            {
                offer.NextOccurrence = offer.StartDate;
            }
            
            await _specialOfferRepository.UpdateAsync(offer);
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        public async Task<bool> DeleteOfferAsync(Guid offerId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var offer = await _specialOfferRepository.GetAsync(offerId);
            var restaurant = await _restaurantRepository.GetAsync(offer.RestaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            await _specialOfferRepository.DeleteAsync(offerId);
            return true;
        }

        #endregion

        #region Ad Requests Management

        public async Task<List<AdRequestDto>> GetAdRequestsAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }
            
            var adRequests = await _adRequestRepository.GetByRestaurantIdAsync(restaurantId);
            return ObjectMapper.Map<List<AdRequest>, List<AdRequestDto>>(adRequests);
        }

        public async Task<AdRequestDto> CreateAdRequestAsync(Guid restaurantId, CreateAdRequestDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            if (!input.ValidateDates())
            {
                throw new ArgumentException("Invalid date range. Start date must be today or later and end date must be after start date.");
            }

            var adRequest = new AdRequest(GuidGenerator.Create())
            {
                RestaurantId = restaurantId,
                Title = input.Title,
                ImageUrl = input.ImageUrl,
                Description = input.Description,
                LinkUrl = input.LinkUrl,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Priority = input.Priority,
                TargetAudience = input.TargetAudience,
                Location = input.Location,
                Budget = input.Budget,
                Status = AdRequestStatus.Pending
            };

            await _adRequestRepository.InsertAsync(adRequest);
            return ObjectMapper.Map<AdRequest, AdRequestDto>(adRequest);
        }

        public async Task<AdRequestDto> UpdateAdRequestAsync(Guid adRequestId, UpdateAdRequestDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var adRequest = await _adRequestRepository.GetAsync(adRequestId);
            var restaurant = await _restaurantRepository.GetAsync(adRequest.RestaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Only allow editing pending requests
            if (adRequest.Status != AdRequestStatus.Pending)
            {
                throw new ArgumentException("Can only edit pending ad requests.");
            }

            if (!input.ValidateDates())
            {
                throw new ArgumentException("Invalid date range. Start date must be today or later and end date must be after start date.");
            }

            adRequest.Title = input.Title;
            adRequest.ImageUrl = input.ImageUrl;
            adRequest.Description = input.Description;
            adRequest.LinkUrl = input.LinkUrl;
            adRequest.StartDate = input.StartDate;
            adRequest.EndDate = input.EndDate;
            adRequest.Priority = input.Priority;
            adRequest.TargetAudience = input.TargetAudience;
            adRequest.Location = input.Location;
            adRequest.Budget = input.Budget;

            await _adRequestRepository.UpdateAsync(adRequest);
            return ObjectMapper.Map<AdRequest, AdRequestDto>(adRequest);
        }

        public async Task<bool> DeleteAdRequestAsync(Guid adRequestId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var adRequest = await _adRequestRepository.GetAsync(adRequestId);
            var restaurant = await _restaurantRepository.GetAsync(adRequest.RestaurantId);
            
            // Verify the restaurant is owned by this user
            if (restaurant.OwnerId != userGuid)
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Only allow deleting pending requests
            if (adRequest.Status != AdRequestStatus.Pending)
            {
                throw new ArgumentException("Can only delete pending ad requests.");
            }

            await _adRequestRepository.DeleteAsync(adRequestId);
            return true;
        }

        #endregion

        #region Enhanced Owner Management Methods

        public async Task<List<RestaurantOwnerDto>> GetAllRestaurantOwnersAsync(int skipCount = 0, int maxResultCount = 100)
        {
            var owners = await _userRepository.GetUsersByRoleAsync("restaurant_owner");

            var ownerDtos = new List<RestaurantOwnerDto>();
            foreach (var owner in owners)
            {
                var ownerDto = ObjectMapper.Map<AppUser, RestaurantOwnerDto>(owner);
                
                // Get managed restaurants
                var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == owner.Id);
                ownerDto.ManagedRestaurantIds = restaurants.Select(r => r.Id).ToList();
                ownerDto.RestaurantCount = restaurants.Count();
                
                ownerDtos.Add(ownerDto);
            }

            return ownerDtos;
        }

        public async Task<RestaurantOwnerDto> GetOwnerDetailsAsync(Guid ownerId)
        {
            var owner = await _userRepository.GetAsync(ownerId);
            
            // Check if user has restaurant_owner role
            var userRoles = await _userManager.GetRolesAsync(owner);
            if (!userRoles.Contains("restaurant_owner"))
            {
                throw new ArgumentException("User is not a restaurant owner");
            }

            var ownerDto = ObjectMapper.Map<AppUser, RestaurantOwnerDto>(owner);
            
            // Get managed restaurants with details
            var restaurants = await _restaurantRepository.GetListWithCategoryAsync(r => r.OwnerId == ownerId);
            ownerDto.ManagedRestaurantIds = restaurants.Select(r => r.Id).ToList();
            ownerDto.ManagedRestaurants = ObjectMapper.Map<List<Restaurant>, List<RestaurantSummaryDto>>(restaurants);
            ownerDto.RestaurantCount = restaurants.Count();
            
            return ownerDto;
        }

        #endregion

        #region Missing Interface Methods Implementation

        public async Task<RestaurantOwnerStatisticsDto> GetOwnerStatisticsAsync(Guid ownerId, DateTime? startDate, DateTime? endDate)
        {
            var owner = await _userRepository.GetAsync(ownerId);
            var userRoles = await _userManager.GetRolesAsync(owner);
            if (!userRoles.Contains("restaurant_owner"))
            {
                throw new ArgumentException("User is not a restaurant owner");
            }

            var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == ownerId);
            var restaurantIds = restaurants.Select(r => r.Id).ToList();

            var orders = await _orderRepository.GetListAsync(o => restaurantIds.Contains(o.RestaurantId));
            
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.CreationTime >= startDate.Value).ToList();
            }
            
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.CreationTime <= endDate.Value).ToList();
            }

            return new RestaurantOwnerStatisticsDto
            {
                OwnerId = ownerId,
                OwnerName = owner.Name,
                GeneratedAt = DateTime.UtcNow,
                TotalRestaurants = restaurants.Count(),
                ActiveRestaurants = restaurants.Count(r => r.IsActive),
                PendingRestaurants = restaurants.Count(r => !r.IsActive),
                TotalOrders = orders.Count(),
                CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                CompletionRate = orders.Any() ? (double)orders.Count(o => o.Status == OrderStatus.Delivered) / orders.Count() * 100 : 0,
                CancellationRate = orders.Any() ? (double)orders.Count(o => o.Status == OrderStatus.Cancelled) / orders.Count() * 100 : 0
            };
        }

        public async Task<List<RestaurantOwnerDto>> SearchOwnersAsync(string searchTerm, string status, DateTime? fromDate, DateTime? toDate, int skipCount = 0, int maxResultCount = 100)
        {
            var owners = await _userRepository.GetUsersByRoleAsync("restaurant_owner");
            
            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                owners = owners.Where(o => 
                    o.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    o.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    o.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Apply date filter
            if (fromDate.HasValue)
            {
                owners = owners.Where(o => o.CreationTime >= fromDate.Value).ToList();
            }
            
            if (toDate.HasValue)
            {
                owners = owners.Where(o => o.CreationTime <= toDate.Value).ToList();
            }

            // Apply pagination
            owners = owners.Skip(skipCount).Take(maxResultCount).ToList();

            var ownerDtos = new List<RestaurantOwnerDto>();
            foreach (var owner in owners)
            {
                var ownerDto = ObjectMapper.Map<AppUser, RestaurantOwnerDto>(owner);
                
                var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == owner.Id);
                ownerDto.ManagedRestaurantIds = restaurants.Select(r => r.Id).ToList();
                ownerDto.RestaurantCount = restaurants.Count();
                
                ownerDtos.Add(ownerDto);
            }

            return ownerDtos;
        }

        public async Task<bool> BulkApproveOwnersAsync(List<Guid> ownerIds)
        {
            foreach (var ownerId in ownerIds)
            {
                var owner = await _userRepository.GetAsync(ownerId);
                owner.IsAdminApproved = true;
                owner.ApprovedTime = DateTime.UtcNow;
                await _userRepository.UpdateAsync(owner);
            }
            return true;
        }

        public async Task<bool> BulkRejectOwnersAsync(List<Guid> ownerIds, string reason)
        {
            foreach (var ownerId in ownerIds)
            {
                var owner = await _userRepository.GetAsync(ownerId);
                owner.IsAdminApproved = false;
                owner.ReviewStatus = ReviewStatus.Rejected;
                owner.ReviewReason = reason;
                await _userRepository.UpdateAsync(owner);
            }
            return true;
        }

        public async Task<bool> BulkActivateOwnersAsync(List<Guid> ownerIds)
        {
            foreach (var ownerId in ownerIds)
            {
                var owner = await _userRepository.GetAsync(ownerId);
                await _userManager.SetLockoutEnabledAsync(owner, false);
                await _userManager.SetLockoutEndDateAsync(owner, null);
                await _userRepository.UpdateAsync(owner);
            }
            return true;
        }

        public async Task<bool> BulkDeactivateOwnersAsync(List<Guid> ownerIds)
        {
            foreach (var ownerId in ownerIds)
            {
                var owner = await _userRepository.GetAsync(ownerId);
                await _userManager.SetLockoutEnabledAsync(owner, true);
                await _userManager.SetLockoutEndDateAsync(owner, DateTimeOffset.MaxValue);
                await _userRepository.UpdateAsync(owner);
            }
            return true;
        }

        public async Task<RestaurantOwnerPerformanceDto> GetOwnerPerformanceAsync(Guid ownerId, DateTime fromDate, DateTime toDate)
        {
            var owner = await _userRepository.GetAsync(ownerId);
            var userRoles = await _userManager.GetRolesAsync(owner);
            if (!userRoles.Contains("restaurant_owner"))
            {
                throw new ArgumentException("User is not a restaurant owner");
            }

            var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == ownerId);
            var restaurantIds = restaurants.Select(r => r.Id).ToList();

            var orders = await _orderRepository.GetListAsync(o => 
                restaurantIds.Contains(o.RestaurantId) && 
                o.CreationTime >= fromDate && 
                o.CreationTime <= toDate);

            return new RestaurantOwnerPerformanceDto
            {
                OwnerId = ownerId,
                OwnerName = owner.Name,
                FromDate = fromDate,
                ToDate = toDate,
                TotalOrders = orders.Count(),
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                AverageRating = restaurants.Any() ? restaurants.Average(r => r.Rating) : 0,
                TotalCustomers = orders.Select(o => o.UserId).Distinct().Count(),
                TotalCommission = orders.Sum(o => o.TotalAmount) * 0.1m, // Assuming 10% commission
                NetProfit = orders.Sum(o => o.TotalAmount) * 0.9m // Assuming 90% net profit
            };
        }

        public async Task<List<RestaurantOwnerDto>> GetTopPerformingOwnersAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var owners = await _userRepository.GetUsersByRoleAsync("restaurant_owner");
            var ownerPerformances = new List<(AppUser Owner, double Performance)>();

            foreach (var owner in owners)
            {
                var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == owner.Id);
                var restaurantIds = restaurants.Select(r => r.Id).ToList();

                var orders = await _orderRepository.GetListAsync(o => restaurantIds.Contains(o.RestaurantId));
                
                if (fromDate.HasValue)
                {
                    orders = orders.Where(o => o.CreationTime >= fromDate.Value).ToList();
                }
                
                if (toDate.HasValue)
                {
                    orders = orders.Where(o => o.CreationTime <= toDate.Value).ToList();
                }

                var performance = orders.Any() ? (double)orders.Sum(o => o.TotalAmount) : 0;
                ownerPerformances.Add((owner, performance));
            }

            var topOwners = ownerPerformances
                .OrderByDescending(p => p.Performance)
                .Take(count)
                .Select(p => p.Owner)
                .ToList();

            var ownerDtos = new List<RestaurantOwnerDto>();
            foreach (var owner in topOwners)
            {
                var ownerDto = ObjectMapper.Map<AppUser, RestaurantOwnerDto>(owner);
                
                var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == owner.Id);
                ownerDto.ManagedRestaurantIds = restaurants.Select(r => r.Id).ToList();
                ownerDto.RestaurantCount = restaurants.Count();
                
                ownerDtos.Add(ownerDto);
            }

            return ownerDtos;
        }

        public async Task<RestaurantOwnerDashboardDto> GetOwnerDashboardAsync(Guid ownerId)
        {
            var owner = await _userRepository.GetAsync(ownerId);
            var userRoles = await _userManager.GetRolesAsync(owner);
            if (!userRoles.Contains("restaurant_owner"))
            {
                throw new ArgumentException("User is not a restaurant owner");
            }

            var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == ownerId);
            var restaurantIds = restaurants.Select(r => r.Id).ToList();

            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var todayOrders = await _orderRepository.GetListAsync(o => 
                restaurantIds.Contains(o.RestaurantId) && 
                o.CreationTime.Date == today);

            var thisMonthOrders = await _orderRepository.GetListAsync(o => 
                restaurantIds.Contains(o.RestaurantId) && 
                o.CreationTime >= thisMonth);

            var lastMonthOrders = await _orderRepository.GetListAsync(o => 
                restaurantIds.Contains(o.RestaurantId) && 
                o.CreationTime >= lastMonth && 
                o.CreationTime < thisMonth);

            return new RestaurantOwnerDashboardDto
            {
                OwnerId = ownerId,
                OwnerName = owner.Name,
                Email = owner.Email,
                PhoneNumber = owner.PhoneNumber,
                LastLoginDate = owner.LastModificationTime ?? owner.CreationTime,
                IsActive = owner.IsActive,
                TotalRestaurants = restaurants.Count(),
                ActiveRestaurants = restaurants.Count(r => r.IsActive),
                PendingRestaurants = restaurants.Count(r => !r.IsActive),
                TodayOrders = todayOrders.Count(),
                TodayRevenue = todayOrders.Sum(o => o.TotalAmount),
                MonthlyOrders = thisMonthOrders.Count(),
                MonthlyRevenue = thisMonthOrders.Sum(o => o.TotalAmount)
            };
        }

        #endregion
    }
}
