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
    public class RestaurantRepository : EfCoreRepository<DeliveryAppDbContext, Restaurant, Guid>, IRestaurantRepository
    {
        public RestaurantRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<Restaurant>> GetListAsync(
            int skipCount,
            int maxResultCount,
            string sorting,
            bool includeDetails = false)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .OrderByDescending(r => r.Rating)
                .Skip(skipCount)
                .Take(maxResultCount)
                .Include(r => r.Address)
                .Include(r => r.Menu)
                .ToListAsync();
        }

        public async Task<List<Restaurant>> SearchAsync(string searchTerm, int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(r => r.Name.Contains(searchTerm) || 
                           r.Description.Contains(searchTerm) ||
                           r.Tags.Any(t => t.Contains(searchTerm)))
                .OrderByDescending(r => r.Rating)
                .Take(maxResultCount)
                .Include(r => r.Address)
                .Include(r => r.Menu)
                .ToListAsync();
        }

        public async Task<List<Restaurant>> GetByCategoryAsync(string category, int skipCount = 0, int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(r => r.Tags.Contains(category))
                .OrderByDescending(r => r.Rating)
                .Skip(skipCount)
                .Take(maxResultCount)
                .Include(r => r.Address)
                .Include(r => r.Menu)
                .Include(r => r.Category)
                .ToListAsync();
        }

        public async Task<List<Restaurant>> GetByCategoryIdAsync(Guid categoryId, int skipCount = 0, int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(r => r.CategoryId == categoryId)
                .OrderByDescending(r => r.Rating)
                .Skip(skipCount)
                .Take(maxResultCount)
                .Include(r => r.Address)
                .Include(r => r.Menu)
                .Include(r => r.Category)
                .ToListAsync();
        }

        public async Task<List<Restaurant>> GetRestaurantsByIdsAsync(List<Guid> restaurantIds)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(r => restaurantIds.Contains(r.Id))
                .Include(r => r.Address)
                .Include(r => r.Menu)
                .Include(r => r.Category)
                .ToListAsync();
        }

        public async Task<Restaurant> GetWithCategoryAsync(Guid id)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(r => r.Id == id)
                .Include(r => r.Address)
                .Include(r => r.Menu)
                .Include(r => r.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Restaurant>> GetListWithCategoryAsync(System.Linq.Expressions.Expression<Func<Restaurant, bool>>? predicate = null)
        {
            var dbSet = await GetDbSetAsync();
            IQueryable<Restaurant> query = dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query
                .Include(r => r.Address)
                .Include(r => r.Menu)
                .Include(r => r.Category)
                .ToListAsync();
        }

        public async Task<Restaurant?> GetWithAddressAsync(Guid restaurantId)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<Restaurant>()
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
        }
    }
}
