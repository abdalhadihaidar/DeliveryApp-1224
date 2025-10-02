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
    public class AdvertisementRepository : EfCoreRepository<DeliveryAppDbContext, Advertisement, Guid>, IAdvertisementRepository
    {
        public AdvertisementRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<Advertisement>> GetListAsync(
            int skipCount,
            int maxResultCount,
            string sorting,
            bool includeDetails = false)
        {
            var dbSet = await GetDbSetAsync();
            
            var query = dbSet.AsQueryable();
            
            if (includeDetails)
            {
                query = query
                    .Include(a => a.Restaurant)
                    .Include(a => a.CreatedBy);
            }
            
            return await query
                .OrderByDescending(a => a.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetActiveAdvertisementsAsync(int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            var now = DateTime.Now;
            
            return await dbSet
                .Where(a => a.IsActive && a.StartDate <= now && a.EndDate >= now)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreationTime)
                .Take(maxResultCount)
                .Include(a => a.Restaurant)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> SearchAsync(string searchTerm, int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(a => a.Title.Contains(searchTerm) || 
                           (a.Description != null && a.Description.Contains(searchTerm)))
                .OrderByDescending(a => a.CreationTime)
                .Take(maxResultCount)
                .Include(a => a.Restaurant)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetByLocationAsync(string location, int skipCount = 0, int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(a => a.Location == location)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .Include(a => a.Restaurant)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetByRestaurantAsync(Guid restaurantId, int skipCount = 0, int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(a => a.RestaurantId == restaurantId)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .Include(a => a.Restaurant)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetExpiredAdvertisementsAsync()
        {
            var dbSet = await GetDbSetAsync();
            var now = DateTime.Now;
            
            return await dbSet
                .Where(a => a.EndDate < now)
                .OrderByDescending(a => a.EndDate)
                .Include(a => a.Restaurant)
                .ToListAsync();
        }

        public async Task<List<Advertisement>> GetUpcomingAdvertisementsAsync()
        {
            var dbSet = await GetDbSetAsync();
            var now = DateTime.Now;
            
            return await dbSet
                .Where(a => a.StartDate > now)
                .OrderBy(a => a.StartDate)
                .Include(a => a.Restaurant)
                .ToListAsync();
        }
    }
} 
