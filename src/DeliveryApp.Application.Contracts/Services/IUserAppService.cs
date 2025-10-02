using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;


namespace DeliveryApp.Application.Contracts.Services
{
    public interface IUserAppService : IApplicationService
    {
        // Profile management
        Task<UserProfileDto> GetUserProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto input);

        // Address management
        Task<List<AddressDto>> GetUserAddressesAsync(Guid userId);
        Task<AddressDto> AddAddressAsync(Guid userId, CreateAddressDto input);
        Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto input);
        Task<bool> DeleteAddressAsync(Guid userId, Guid addressId);
        Task<AddressDto> SetDefaultAddressAsync(Guid userId, Guid addressId);

        // Payment method management
        Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(Guid userId);
        Task<PaymentMethodDto> AddPaymentMethodAsync(Guid userId, CreatePaymentMethodDto input);
        Task<bool> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId);
        Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId);

        // Favorite restaurants
        Task<List<Guid>> GetFavoriteRestaurantsAsync(Guid userId);
        Task<bool> AddFavoriteRestaurantAsync(Guid userId, Guid restaurantId);
        Task<bool> RemoveFavoriteRestaurantAsync(Guid userId, Guid restaurantId);
        Task<bool> IsFavoriteRestaurantAsync(Guid userId, Guid restaurantId);

        // Authentication methods
        Task<UserProfileDto> ValidateUserAsync(LoginDto loginDto);
        // NOTE: GenerateJwtToken removed - use AuthService or MobileAuthService for token generation

        /// <summary>
        /// List all users (admin only)
        /// </summary>
        Task<PagedResultDto<UserDto>> GetListAsync(PagedAndSortedResultRequestDto input);

        /// <summary>
        /// Create a new user (admin only)
        /// </summary>
        Task<UserDto> CreateAsync(CreateUserDto input);

        /// <summary>
        /// Update a user (admin only)
        /// </summary>
        Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input);

        /// <summary>
        /// Delete a user (admin only) - Soft delete
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Restore a soft-deleted user (admin only)
        /// </summary>
        Task RestoreAsync(Guid id);

        /// <summary>
        /// Accept a user (KYC/activate)
        /// </summary>
        Task<UserDto> AcceptUserAsync(Guid userId, string reason);

        /// <summary>
        /// Reject a user (KYC/deactivate)
        /// </summary>
        Task<UserDto> RejectUserAsync(Guid userId, string reason);

        /// <summary>
        /// Get all available roles (admin only)
        /// </summary>
        Task<List<string>> GetAllRolesAsync();

        /// <summary>
        /// Get restaurants by owner ID (for restaurant owner review)
        /// </summary>
        Task<List<RestaurantDto>> GetRestaurantsByOwnerAsync(Guid ownerId);

        /// <summary>
        /// Get all users for debugging purposes
        /// </summary>
        Task<List<UserDto>> GetAllUsersForDebugAsync();

        /// <summary>
        /// Get a specific user by ID for debugging purposes
        /// </summary>
        Task<UserDto?> GetUserByIdForDebugAsync(Guid userId);

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        Task<UserDto> GetAsync(Guid id);

        /// <summary>
        /// Get soft-deleted users for debugging purposes
        /// </summary>
        Task<List<UserDto>> GetSoftDeletedUsersForDebugAsync();
        Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(Guid userId);
    }
} 
