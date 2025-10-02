using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [RemoteService]
    [Route("api/app/restaurant-owner")]
    [Authorize(Roles = "restaurant_owner")]
    public class RestaurantOwnerController : AbpController
    {
        private readonly IRestaurantOwnerAppService _restaurantOwnerAppService;

        public RestaurantOwnerController(IRestaurantOwnerAppService restaurantOwnerAppService)
        {
            _restaurantOwnerAppService = restaurantOwnerAppService;
        }

        [HttpGet("profile")]
        public async Task<RestaurantOwnerDto> GetProfile()
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.GetProfileAsync(userId);
        }

        [HttpPut("profile")]
        public async Task<RestaurantOwnerDto> UpdateProfile([FromBody] UpdateUserDto input)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            // Set the user ID from the token
            input.UserId = userId;
            
            return await _restaurantOwnerAppService.UpdateProfileAsync(input);
        }

        [HttpGet("restaurants")]
        public async Task<List<RestaurantDto>> GetManagedRestaurants()
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.GetManagedRestaurantsAsync(userId);
        }

        [HttpGet("restaurants/{restaurantId}")]
        public async Task<RestaurantDto> GetRestaurantDetails(Guid restaurantId)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.GetRestaurantDetailsAsync(restaurantId, userId);
        }

        [HttpPost("restaurants")]
        public async Task<RestaurantDto> CreateRestaurant([FromBody] CreateRestaurantDto input)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.CreateRestaurantAsync(input, userId);
        }

        [HttpPut("restaurants/{restaurantId}")]
        public async Task<RestaurantDto> UpdateRestaurant(Guid restaurantId, [FromBody] UpdateRestaurantDto input)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.UpdateRestaurantAsync(restaurantId, input, userId);
        }

        [HttpGet("restaurants/{restaurantId}/menu")]
        public async Task<List<MenuItemDto>> GetMenuItems(Guid restaurantId)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.GetMenuItemsAsync(restaurantId, userId);
        }

        [HttpPost("restaurants/{restaurantId}/menu")]
        public async Task<MenuItemDto> AddMenuItem(Guid restaurantId, [FromBody] CreateMenuItemDto input)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.AddMenuItemAsync(restaurantId, input, userId);
        }

        [HttpGet("restaurants/{restaurantId}/categories")]
        public async Task<List<string>> GetRestaurantCategories(Guid restaurantId)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.GetRestaurantCategoriesAsync(restaurantId, userId);
        }

        [HttpPut("restaurants/{restaurantId}/menu/{menuItemId}")]
        public async Task<MenuItemDto> UpdateMenuItem(Guid restaurantId, Guid menuItemId, [FromBody] UpdateMenuItemDto input)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.UpdateMenuItemAsync(restaurantId, menuItemId, input, userId);
        }

        [HttpDelete("restaurants/{restaurantId}/menu/{menuItemId}")]
        public async Task<bool> DeleteMenuItem(Guid restaurantId, Guid menuItemId)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.DeleteMenuItemAsync(restaurantId, menuItemId, userId);
        }

        [HttpPut("restaurants/{restaurantId}/menu/{menuItemId}/availability")]
        public async Task<bool> UpdateMenuItemAvailability(Guid restaurantId, Guid menuItemId, [FromBody] bool isAvailable)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.UpdateMenuItemAvailabilityAsync(restaurantId, menuItemId, isAvailable, userId);
        }

        [HttpGet("restaurants/{restaurantId}/orders")]
        public async Task<List<OrderDto>> GetRestaurantOrders(Guid restaurantId, [FromQuery] string status = null)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            OrderStatus? orderStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                orderStatus = parsedStatus;
            }
            
            return await _restaurantOwnerAppService.GetRestaurantOrdersAsync(restaurantId, orderStatus, userId);
        }

        [HttpGet("orders/{orderId}")]
        public async Task<OrderDto> GetOrderDetails(Guid orderId)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.GetOrderDetailsAsync(orderId, userId);
        }

        [HttpPut("orders/{orderId}/status")]
        public async Task<OrderDto> UpdateOrderStatus(Guid orderId, [FromBody] string status)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                throw new ArgumentException($"Invalid order status: {status}");
            }
            
            return await _restaurantOwnerAppService.UpdateOrderStatusAsync(orderId, orderStatus, userId);
        }

        [HttpPost("orders/{orderId}/cancel")]
        public async Task<bool> CancelOrder(Guid orderId)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.CancelOrderAsync(orderId, userId);
        }

        [HttpGet("restaurants/{restaurantId}/statistics")]
        public async Task<RestaurantStatisticsDto> GetRestaurantStatistics(
            Guid restaurantId, 
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            // Extract user ID from JWT token - try both nameid and NameIdentifier claims
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
            
            return await _restaurantOwnerAppService.GetRestaurantStatisticsAsync(restaurantId, startDate, endDate, userId);
        }
    }
}
