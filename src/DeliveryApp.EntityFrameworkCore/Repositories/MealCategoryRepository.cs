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
    public class MealCategoryRepository : EfCoreRepository<DeliveryAppDbContext, MealCategory, Guid>, IMealCategoryRepository
    {
        public MealCategoryRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<List<MealCategory>> GetByRestaurantIdAsync(Guid restaurantId)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(mc => mc.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<List<MealCategory>> GetByRestaurantIdOrderedAsync(Guid restaurantId)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(mc => mc.RestaurantId == restaurantId)
                .OrderBy(mc => mc.SortOrder)
                .ThenBy(mc => mc.Name)
                .ToListAsync();
        }

        public async Task<bool> IsNameUniqueInRestaurantAsync(string name, Guid restaurantId, Guid? excludeId = null)
        {
            var query = await GetQueryableAsync();
            
            var existingCategory = await query
                .FirstOrDefaultAsync(mc => mc.RestaurantId == restaurantId && 
                                         mc.Name == name && 
                                         (excludeId == null || mc.Id != excludeId));
            
            return existingCategory == null;
        }
    }
}
