using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DeliveryApp.EntityFrameworkCore.Repositories
{
    public class RestaurantCategoryRepository : EfCoreRepository<DeliveryAppDbContext, RestaurantCategory, Guid>, IRestaurantCategoryRepository
    {
        public RestaurantCategoryRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<List<RestaurantCategory>> GetActiveCategoriesAsync()
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<List<RestaurantCategory>> GetActiveCategoriesOrderedAsync()
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<RestaurantCategory?> GetByNameAsync(string name)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<List<RestaurantCategory>> GetCategoriesWithRestaurantCountAsync()
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Include(c => c.Restaurants)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
        {
            var query = await GetQueryableAsync();
            
            var normalizedName = name.Trim().ToLowerInvariant();
            
            var existingCategory = await query
                .Where(c => c.Name.ToLower() == normalizedName)
                .Where(c => excludeId == null || c.Id != excludeId)
                .FirstOrDefaultAsync();
                
            return existingCategory == null;
        }
    }
}
