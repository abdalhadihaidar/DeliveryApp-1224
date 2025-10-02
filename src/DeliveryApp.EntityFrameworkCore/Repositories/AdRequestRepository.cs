using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DeliveryApp.EntityFrameworkCore.Repositories
{
    public class AdRequestRepository : EfCoreRepository<DeliveryAppDbContext, AdRequest, Guid>, IAdRequestRepository
    {
        public AdRequestRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<List<AdRequest>> GetByRestaurantIdAsync(Guid restaurantId)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(ar => ar.RestaurantId == restaurantId)
                .Include(ar => ar.Restaurant)
                .Include(ar => ar.ReviewedBy)
                .OrderByDescending(ar => ar.CreationTime)
                .ToListAsync();
        }

        public async Task<List<AdRequest>> GetByStatusAsync(AdRequestStatus status)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(ar => ar.Status == status)
                .Include(ar => ar.Restaurant)
                .Include(ar => ar.ReviewedBy)
                .OrderByDescending(ar => ar.CreationTime)
                .ToListAsync();
        }

        public async Task<List<AdRequest>> GetPendingRequestsAsync()
        {
            return await GetByStatusAsync(AdRequestStatus.Pending);
        }

        public async Task<List<AdRequest>> GetListWithRestaurantAsync(int skipCount, int maxResultCount, string? searchTerm = null, AdRequestStatus? status = null)
        {
            var query = await GetQueryableAsync();
            
            query = query.Include(ar => ar.Restaurant)
                        .Include(ar => ar.ReviewedBy);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(ar => ar.Title.Contains(searchTerm) || 
                                        ar.Restaurant.Name.Contains(searchTerm));
            }

            if (status.HasValue)
            {
                query = query.Where(ar => ar.Status == status.Value);
            }

            return await query
                .OrderByDescending(ar => ar.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }
    }
}
