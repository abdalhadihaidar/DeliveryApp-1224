using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Web.Controllers
{
    [RemoteService]
    [Route("api/app/admin/restaurants")]
    [Authorize(Roles = "admin,manager")]
    public class AdminRestaurantController : AbpController
    {
        private readonly IAdminRestaurantAppService _adminRestaurantAppService;

        public AdminRestaurantController(IAdminRestaurantAppService adminRestaurantAppService)
        {
            _adminRestaurantAppService = adminRestaurantAppService;
        }

        [HttpGet]
        public async Task<Application.Contracts.Dtos.PagedResultDto<RestaurantDto>> GetAllRestaurants(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string sortBy = "id", 
            [FromQuery] string sortOrder = "desc")
        {
            return await _adminRestaurantAppService.GetAllRestaurantsAsync(page, pageSize, sortBy, sortOrder);
        }

        [HttpGet("pending")]
        public async Task<RestaurantDto[]> GetPendingRestaurants()
        {
            var pendingRestaurants = await _adminRestaurantAppService.GetPendingRestaurantsAsync();
            return pendingRestaurants.ToArray();
        }

        [HttpGet("{restaurantId}")]
        public async Task<RestaurantDto> GetRestaurantDetails(Guid restaurantId)
        {
            return await _adminRestaurantAppService.GetRestaurantDetailsAsync(restaurantId);
        }

        [HttpPost("{restaurantId}/approve")]
        public async Task<RestaurantDto> ApproveRestaurant(Guid restaurantId)
        {
            return await _adminRestaurantAppService.ApproveRestaurantAsync(restaurantId);
        }

        [HttpPost("{restaurantId}/reject")]
        public async Task<RestaurantDto> RejectRestaurant(Guid restaurantId, [FromBody] RejectRestaurantDto input)
        {
            return await _adminRestaurantAppService.RejectRestaurantAsync(restaurantId, input.Reason);
        }

        [HttpPut("{restaurantId}/commission")]
        public async Task<RestaurantDto> UpdateCommission(Guid restaurantId, [FromBody] decimal commissionPercent)
        {
            return await _adminRestaurantAppService.UpdateCommissionAsync(restaurantId, commissionPercent);
        }

        [HttpPut("{restaurantId}/activation")]
        public async Task<RestaurantDto> ToggleActivation(Guid restaurantId, [FromBody] bool isActive)
        {
            return await _adminRestaurantAppService.ToggleActivationAsync(restaurantId, isActive);
        }

        // Restaurant management
        [HttpPost]
        public async Task<RestaurantDto> CreateRestaurant([FromBody] CreateRestaurantRequestDto input)
        {
            return await _adminRestaurantAppService.CreateRestaurantAsync(input.Restaurant, input.OwnerId);
        }

        [HttpPut("{restaurantId}")]
        public async Task<RestaurantDto> UpdateRestaurant(Guid restaurantId, [FromBody] UpdateRestaurantDto input)
        {
            return await _adminRestaurantAppService.UpdateRestaurantAsync(restaurantId, input);
        }

        [HttpDelete("{restaurantId}")]
        public async Task<bool> DeleteRestaurant(Guid restaurantId)
        {
            return await _adminRestaurantAppService.DeleteRestaurantAsync(restaurantId);
        }

        // Restaurant owner management
        [HttpGet("owners")]
        public async Task<RestaurantOwnerDto[]> GetAllRestaurantOwners()
        {
            var owners = await _adminRestaurantAppService.GetAllRestaurantOwnersAsync();
            return owners.ToArray();
        }

        [HttpGet("owners/{ownerId}")]
        public async Task<RestaurantOwnerDetailsDto> GetRestaurantOwnerDetails(Guid ownerId)
        {
            return await _adminRestaurantAppService.GetRestaurantOwnerDetailsAsync(ownerId);
        }

        [HttpPost("owners")]
        public async Task<RestaurantOwnerDto> CreateRestaurantOwner([FromBody] CreateRestaurantOwnerDto input)
        {
            return await _adminRestaurantAppService.CreateRestaurantOwnerAsync(input);
        }

        [HttpPut("owners/{ownerId}")]
        public async Task<RestaurantOwnerDto> UpdateRestaurantOwner(Guid ownerId, [FromBody] UpdateRestaurantOwnerDto input)
        {
            return await _adminRestaurantAppService.UpdateRestaurantOwnerAsync(ownerId, input);
        }

        [HttpDelete("owners/{ownerId}")]
        public async Task<bool> DeleteRestaurantOwner(Guid ownerId)
        {
            return await _adminRestaurantAppService.DeleteRestaurantOwnerAsync(ownerId);
        }

        // Restaurant-Owner linking
        [HttpPost("{restaurantId}/assign-owner")]
        public async Task<RestaurantDto> AssignRestaurantToOwner(Guid restaurantId, [FromBody] AssignOwnerRequestDto input)
        {
            return await _adminRestaurantAppService.AssignRestaurantToOwnerAsync(restaurantId, input.OwnerId);
        }

        [HttpPut("{restaurantId}/change-owner")]
        public async Task<RestaurantDto> ChangeRestaurantOwner(Guid restaurantId, [FromBody] ChangeOwnerRequestDto input)
        {
            return await _adminRestaurantAppService.ChangeRestaurantOwnerAsync(restaurantId, input.NewOwnerId);
        }

        [HttpGet("by-owner/{ownerId}")]
        public async Task<RestaurantDto[]> GetRestaurantsByOwner(Guid ownerId)
        {
            var restaurants = await _adminRestaurantAppService.GetRestaurantsByOwnerAsync(ownerId);
            return restaurants.ToArray();
        }
    }

    public class RejectRestaurantDto
    {
        public string Reason { get; set; }
    }

    public class CreateRestaurantRequestDto
    {
        public CreateRestaurantDto Restaurant { get; set; }
        public Guid OwnerId { get; set; }
    }

    public class AssignOwnerRequestDto
    {
        public Guid OwnerId { get; set; }
    }

    public class ChangeOwnerRequestDto
    {
        public Guid NewOwnerId { get; set; }
    }
}
