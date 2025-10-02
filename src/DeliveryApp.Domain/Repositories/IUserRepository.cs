using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using DeliveryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace DeliveryApp.Domain.Repositories
{
    public interface IUserRepository : IRepository<AppUser, Guid>
    {
        Task<DbContext> GetDbContextAsync();
        Task<AppUser> GetByIdWithDetailsAsync(Guid userId);
        Task<List<Address>> GetUserAddressesAsync(Guid userId);
        Task UnsetDefaultAddressAsync(Guid userId);
        Task AddAddressAsync(Address address);
        Task UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(Address address);
        Task<List<PaymentMethod>> GetUserPaymentMethodsAsync(Guid userId);
        Task UnsetDefaultPaymentMethodAsync(Guid userId);
        Task AddPaymentMethodAsync(PaymentMethod paymentMethod);
        Task UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task DeletePaymentMethodAsync(PaymentMethod paymentMethod);
        Task<List<Guid>> GetUserFavoriteRestaurantsAsync(Guid userId);
        Task AddFavoriteRestaurantAsync(Guid userId, Guid restaurantId);
        Task RemoveFavoriteRestaurantAsync(Guid userId, Guid restaurantId);
        Task<bool> IsFavoriteRestaurantAsync(Guid userId, Guid restaurantId);
        Task<AppUser> GetUserWithDetailsAsync(Guid userId);
        /// <summary>
        /// Get a paged list of users with optional filtering and sorting
        /// </summary>
        Task<List<AppUser>> GetPagedListAsync(int skipCount, int maxResultCount, string sorting = null, string filter = null);
        Task<int> GetCountAsync(string filter = null);

        /// <summary>
        /// Create a new user
        /// </summary>
        Task<AppUser> CreateUserAsync(AppUser user, string password, string role);

        /// <summary>
        /// Update an existing user
        /// </summary>
        Task<AppUser> UpdateUserAsync(AppUser user, string role = null);

        /// <summary>
        /// Get a user by phone number
        /// </summary>
        Task<AppUser> GetByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Delete a user by id
        /// </summary>
        Task DeleteUserAsync(Guid userId);

        /// <summary>
        /// Get users by role
        /// </summary>
        Task<List<AppUser>> GetUsersByRoleAsync(string roleName);
    }
}
