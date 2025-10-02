using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using System.Linq;
using DeliveryApp.Application.Contracts.Services;

namespace DeliveryApp.Web.Controllers
{
    [RemoteService]
    [Route("api/admin/restaurant-owners")]
    [Authorize(Roles = "admin")]
    public class AdminRestaurantOwnerController : AbpController
    {
        private readonly IUserAppService _userAppService;
        private readonly IRestaurantAppService _restaurantAppService;
        private readonly IRestaurantOwnerAppService _restaurantOwnerAppService;

        public AdminRestaurantOwnerController(
            IUserAppService userAppService,
            IRestaurantAppService restaurantAppService,
            IRestaurantOwnerAppService restaurantOwnerAppService)
        {
            _userAppService = userAppService;
            _restaurantAppService = restaurantAppService;
            _restaurantOwnerAppService = restaurantOwnerAppService;
        }

        /// <summary>
        /// Get all restaurant owners with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<Abp.Application.Services.Dto.PagedResultDto<RestaurantOwnerAdminDto>> GetRestaurantOwners(
            [FromQuery] int skipCount = 0,
            [FromQuery] int maxResultCount = 10,
            [FromQuery] string sorting = "CreationTime desc",
            [FromQuery] string filter = "",
            [FromQuery] string status = "all")
        {
            var input = new Application.Contracts.Dtos.PagedAndSortedResultRequestDto
            {
                SkipCount = skipCount,
                MaxResultCount = maxResultCount,
                Sorting = sorting
            };

            // Get all users with restaurant_owner role
            var users = await _userAppService.GetListAsync(input);
            
            // Filter for restaurant owners
            var restaurantOwners = new List<RestaurantOwnerAdminDto>();
            
            foreach (var user in users.Items)
            {
                if (user.UserType == "restaurant_owner")
                {
                    // Get restaurants managed by this owner
                    var restaurants = await _restaurantAppService.GetRestaurantsByOwnerAsync(user.Id.ToString());
                    
                    // Calculate statistics
                    var totalOrders = 0;
                    var totalRevenue = 0.0m;
                    var lastLogin = user.LastLoginTime;
                    
                    foreach (var restaurant in restaurants)
                    {
                        // Get restaurant statistics
                        var stats = await _restaurantAppService.GetRestaurantStatisticsAsync(restaurant.Id);
                        totalOrders += stats.TotalOrders;
                        totalRevenue += (decimal)stats.TotalRevenue;
                    }

                    var ownerDto = new RestaurantOwnerAdminDto
                    {
                        Id = user.Id.ToString(),
                        Name = user.Name ?? "N/A",
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        CreationTime = user.CreationTime,
                        LastLoginTime = lastLogin,
                        IsActive = user.IsActive,
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                        UserType = user.UserType,
                        ManagedRestaurants = restaurants.Count(),
                        TotalOrders = totalOrders,
                        TotalRevenue = totalRevenue,
                        Status = DetermineOwnerStatus(user),
                        VerificationStatus = DetermineVerificationStatus(user),
                        ProfileComplete = IsProfileComplete(user)
                    };

                    // Apply filters
                    if (string.IsNullOrEmpty(filter) || 
                        ownerDto.Name.ToLower().Contains(filter.ToLower()) ||
                        ownerDto.Email.ToLower().Contains(filter.ToLower()) ||
                        ownerDto.PhoneNumber?.Contains(filter) == true)
                    {
                        if (status == "all" || ownerDto.Status == status)
                        {
                            restaurantOwners.Add(ownerDto);
                        }
                    }
                }
            }

            return new Abp.Application.Services.Dto.PagedResultDto<RestaurantOwnerAdminDto>
            {
                Items = restaurantOwners,
                TotalCount = restaurantOwners.Count
            };
        }

        /// <summary>
        /// Get restaurant owner details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<RestaurantOwnerAdminDto> GetRestaurantOwnerDetails(string id)
        {
            var user = await _userAppService.GetAsync(Guid.Parse(id));
            
            if (user.UserType != "restaurant_owner")
            {
                throw new UserFriendlyException("User is not a restaurant owner");
            }

            // Get restaurants managed by this owner
            var restaurants = await _restaurantAppService.GetRestaurantsByOwnerAsync(user.Id.ToString());
            
            // Calculate statistics
            var totalOrders = 0;
            var totalRevenue = 0.0m;
            var lastLogin = user.LastLoginTime;
            
            foreach (var restaurant in restaurants)
            {
                var stats = await _restaurantAppService.GetRestaurantStatisticsAsync(restaurant.Id);
                totalOrders += stats.TotalOrders;
                totalRevenue += (decimal)stats.TotalRevenue;
            }

            return new RestaurantOwnerAdminDto
            {
                Id = user.Id.ToString(),
                Name = user.Name ?? "N/A",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreationTime = user.CreationTime,
                LastLoginTime = lastLogin,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                UserType = user.UserType,
                ManagedRestaurants = restaurants.Count(),
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                Status = DetermineOwnerStatus(user),
                VerificationStatus = DetermineVerificationStatus(user),
                ProfileComplete = IsProfileComplete(user),
                Restaurants = restaurants
            };
        }

        /// <summary>
        /// Get restaurants managed by a specific owner
        /// </summary>
        [HttpGet("{id}/restaurants")]
        public async Task<List<RestaurantDto>> GetOwnerRestaurants(string id)
        {
            return await _restaurantAppService.GetRestaurantsByOwnerAsync(id);
        }

        /// <summary>
        /// Approve a restaurant owner
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<RestaurantOwnerAdminDto> ApproveRestaurantOwner(string id, [FromBody] ApproveOwnerDto input)
        {
            var user = await _userAppService.GetAsync(Guid.Parse(id));
            
            if (user.UserType != "restaurant_owner")
            {
                throw new UserFriendlyException("User is not a restaurant owner");
            }

            // Approve the user
            var updateInput = new UpdateUserDto
            {
                UserId = id,
                IsActive = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            await _userAppService.UpdateAsync(Guid.Parse(id), updateInput);

            // Return updated owner details
            return await GetRestaurantOwnerDetails(id);
        }

        /// <summary>
        /// Reject a restaurant owner
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<RestaurantOwnerAdminDto> RejectRestaurantOwner(string id, [FromBody] RejectOwnerDto input)
        {
            var user = await _userAppService.GetAsync(Guid.Parse(id));
            
            if (user.UserType != "restaurant_owner")
            {
                throw new UserFriendlyException("User is not a restaurant owner");
            }

            // Deactivate the user
            var updateInput = new UpdateUserDto
            {
                UserId = id,
                IsActive = false
            };

            await _userAppService.UpdateAsync(Guid.Parse(id), updateInput);

            // Return updated owner details
            return await GetRestaurantOwnerDetails(id);
        }

        /// <summary>
        /// Activate a restaurant owner
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<RestaurantOwnerAdminDto> ActivateRestaurantOwner(string id)
        {
            var user = await _userAppService.GetAsync(Guid.Parse(id));
            
            if (user.UserType != "restaurant_owner")
            {
                throw new UserFriendlyException("User is not a restaurant owner");
            }

            var updateInput = new UpdateUserDto
            {
                UserId = id,
                IsActive = true
            };

            await _userAppService.UpdateAsync(Guid.Parse(id), updateInput);

            return await GetRestaurantOwnerDetails(id);
        }

        /// <summary>
        /// Deactivate a restaurant owner
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<RestaurantOwnerAdminDto> DeactivateRestaurantOwner(string id, [FromBody] DeactivateOwnerDto input)
        {
            var user = await _userAppService.GetAsync(Guid.Parse(id));
            
            if (user.UserType != "restaurant_owner")
            {
                throw new UserFriendlyException("User is not a restaurant owner");
            }

            var updateInput = new UpdateUserDto
            {
                UserId = id,
                IsActive = false
            };

            await _userAppService.UpdateAsync(Guid.Parse(id), updateInput);

            return await GetRestaurantOwnerDetails(id);
        }

        /// <summary>
        /// Get restaurant owner statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<AdminRestaurantOwnerStatisticsDto> GetRestaurantOwnerStatistics()
        {
            var input = new Application.Contracts.Dtos.PagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 1000,
                Sorting = "CreationTime desc"
            };

            var users = await _userAppService.GetListAsync(input);
            var restaurantOwners = users.Items.Where(u => u.UserType == "restaurant_owner").ToList();

            var statistics = new AdminRestaurantOwnerStatisticsDto
            {
                TotalOwners = restaurantOwners.Count,
                ActiveOwners = restaurantOwners.Count(u => u.IsActive && u.EmailConfirmed && u.PhoneNumberConfirmed),
                PendingApprovals = restaurantOwners.Count(u => !u.EmailConfirmed || !u.PhoneNumberConfirmed),
                InactiveOwners = restaurantOwners.Count(u => !u.IsActive),
                TotalRestaurants = 0,
                TotalRevenue = 0.0m,
                AverageRating = 0.0
            };

            // Calculate additional statistics and get top performers
            var ownerDetails = new List<RestaurantOwnerAdminDto>();
            
            foreach (var owner in restaurantOwners)
            {
                var restaurants = await _restaurantAppService.GetRestaurantsByOwnerAsync(owner.Id.ToString());
                statistics.TotalRestaurants += restaurants.Count();

                var totalRevenue = 0.0m;
                foreach (var restaurant in restaurants)
                {
                    var stats = await _restaurantAppService.GetRestaurantStatisticsAsync(restaurant.Id);
                    totalRevenue += (decimal)stats.TotalRevenue;
                }
                statistics.TotalRevenue += totalRevenue;

                // Create owner detail for top performers calculation
                var ownerDetail = new RestaurantOwnerAdminDto
                {
                    Id = owner.Id.ToString(),
                    Name = owner.Name ?? "N/A",
                    Email = owner.Email,
                    PhoneNumber = owner.PhoneNumber,
                    CreationTime = owner.CreationTime,
                    LastLoginTime = owner.LastLoginTime,
                    IsActive = owner.IsActive,
                    EmailConfirmed = owner.EmailConfirmed,
                    PhoneNumberConfirmed = owner.PhoneNumberConfirmed,
                    UserType = owner.UserType,
                    ManagedRestaurants = restaurants.Count(),
                    TotalOrders = 0, // Would need order repository for real data
                    TotalRevenue = totalRevenue,
                    Status = DetermineOwnerStatus(owner),
                    VerificationStatus = DetermineVerificationStatus(owner),
                    ProfileComplete = IsProfileComplete(owner)
                };
                ownerDetails.Add(ownerDetail);
            }

            // Get top 5 performing owners
            statistics.TopPerformingOwners = ownerDetails
                .OrderByDescending(o => o.TotalRevenue)
                .Take(5)
                .ToList();

            return statistics;
        }

        // Helper methods
        private string DetermineOwnerStatus(UserDto user)
        {
            if (!user.IsActive) return "inactive";
            if (user.EmailConfirmed && user.PhoneNumberConfirmed) return "active";
            if (!user.EmailConfirmed || !user.PhoneNumberConfirmed) return "pending";
            return "pending";
        }

        private string DetermineVerificationStatus(UserDto user)
        {
            if (user.EmailConfirmed && user.PhoneNumberConfirmed) return "verified";
            if (!user.EmailConfirmed || !user.PhoneNumberConfirmed) return "pending";
            return "pending";
        }

        private bool IsProfileComplete(UserDto user)
        {
            return !string.IsNullOrEmpty(user.Name) && 
                   !string.IsNullOrEmpty(user.Email) && 
                   !string.IsNullOrEmpty(user.PhoneNumber);
        }
    }


    public class ApproveOwnerDto
    {
        public string Reason { get; set; }
        public bool SendNotification { get; set; } = true;
    }

    public class RejectOwnerDto
    {
        public string Reason { get; set; }
        public bool SendNotification { get; set; } = true;
    }

    public class DeactivateOwnerDto
    {
        public string Reason { get; set; }
        public bool SendNotification { get; set; } = true;
    }

    // RestaurantOwnerStatisticsDto is defined in Application.Contracts.Dtos
}
