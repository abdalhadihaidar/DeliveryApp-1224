using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Services
{
    [RemoteService]
    public interface IAdminRestaurantAppService : IApplicationService
    {
        // Get all restaurants for admin management
        Task<PagedResultDto<RestaurantDto>> GetAllRestaurantsAsync(int page = 1, int pageSize = 10, string sortBy = "id", string sortOrder = "desc");
        
        // Get pending approval restaurants
        Task<List<RestaurantDto>> GetPendingRestaurantsAsync();
        
        // Approve restaurant
        Task<RestaurantDto> ApproveRestaurantAsync(Guid restaurantId);
        
        // Reject restaurant
        Task<RestaurantDto> RejectRestaurantAsync(Guid restaurantId, string reason);
        
        // Update commission
        Task<RestaurantDto> UpdateCommissionAsync(Guid restaurantId, decimal commissionPercent);
        
        // Toggle activation
        Task<RestaurantDto> ToggleActivationAsync(Guid restaurantId, bool isActive);
        
        // Get restaurant details for admin
        Task<RestaurantDto> GetRestaurantDetailsAsync(Guid restaurantId);
        
        // Restaurant creation and management
        Task<RestaurantDto> CreateRestaurantAsync(CreateRestaurantDto input, Guid ownerId);
        Task<RestaurantDto> UpdateRestaurantAsync(Guid restaurantId, UpdateRestaurantDto input);
        Task<bool> DeleteRestaurantAsync(Guid restaurantId);
        
        // Restaurant owner management
        Task<RestaurantOwnerDto> CreateRestaurantOwnerAsync(CreateRestaurantOwnerDto input);
        Task<RestaurantOwnerDto> UpdateRestaurantOwnerAsync(Guid ownerId, UpdateRestaurantOwnerDto input);
        Task<bool> DeleteRestaurantOwnerAsync(Guid ownerId);
        Task<List<RestaurantOwnerDto>> GetAllRestaurantOwnersAsync();
        Task<RestaurantOwnerDetailsDto> GetRestaurantOwnerDetailsAsync(Guid ownerId);
        
        // Link restaurant to owner
        Task<RestaurantDto> AssignRestaurantToOwnerAsync(Guid restaurantId, Guid ownerId);
        Task<RestaurantDto> ChangeRestaurantOwnerAsync(Guid restaurantId, Guid newOwnerId);
        
        // Get restaurants by owner
        Task<List<RestaurantDto>> GetRestaurantsByOwnerAsync(Guid ownerId);
    }
}
