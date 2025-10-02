using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IRestaurantRepository : IRepository<Restaurant, Guid>
    {
        Task<List<Restaurant>> GetListAsync(
            int skipCount,
            int maxResultCount,
            string sorting,
            bool includeDetails = false
        );

        Task<List<Restaurant>> SearchAsync(string searchTerm, int maxResultCount = 10);
        
        Task<List<Restaurant>> GetByCategoryAsync(string category, int skipCount = 0, int maxResultCount = 10);
        Task<List<Restaurant>> GetByCategoryIdAsync(Guid categoryId, int skipCount = 0, int maxResultCount = 10);
        Task<List<Restaurant>> GetRestaurantsByIdsAsync(List<Guid> restaurantIds);

        /// <summary>
        /// Get restaurant with address information
        /// </summary>
        Task<Restaurant?> GetWithAddressAsync(Guid restaurantId);
        
        // Methods that include Category navigation property
        Task<Restaurant> GetWithCategoryAsync(Guid id);
        Task<List<Restaurant>> GetListWithCategoryAsync(System.Linq.Expressions.Expression<Func<Restaurant, bool>>? predicate = null);
    }
}
