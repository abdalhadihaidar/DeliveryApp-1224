using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using System.Text.Json;

namespace DeliveryApp.EntityFrameworkCore.Repositories
{
    public class SpecialOfferRepository : EfCoreRepository<DeliveryAppDbContext, SpecialOffer, Guid>, ISpecialOfferRepository
    {
        public SpecialOfferRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<SpecialOffer>> GetActiveOffersAsync(Guid restaurantId, DateTime? asOf = null)
        {
            var dbSet = await GetDbSetAsync();
            var now = asOf ?? DateTime.Now;
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.Status == OfferStatus.Active && 
                                        o.IsActive &&
                                        o.StartDate <= now && 
                                        o.EndDate >= now)
                               .OrderByDescending(o => o.Priority)
                               .ThenBy(o => o.StartDate)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetListAsync(int skipCount, int maxResultCount, string sorting, Guid? restaurantId = null)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.AsQueryable();
            
            if (restaurantId.HasValue)
            {
                query = query.Where(o => o.RestaurantId == restaurantId.Value);
            }
            
            query = sorting switch
            {
                "StartDate" => query.OrderBy(o => o.StartDate),
                "EndDate" => query.OrderBy(o => o.EndDate),
                "Priority" => query.OrderByDescending(o => o.Priority),
                "Status" => query.OrderBy(o => o.Status),
                "CreationTime" => query.OrderByDescending(o => o.CreationTime),
                _ => query.OrderByDescending(o => o.CreationTime)
            };
            
            return await query.Skip(skipCount).Take(maxResultCount).ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetOffersByStatusAsync(Guid restaurantId, string status)
        {
            var dbSet = await GetDbSetAsync();
            if (Enum.TryParse<OfferStatus>(status, out var statusEnum))
            {
                return await dbSet.Where(o => o.RestaurantId == restaurantId && o.Status == statusEnum)
                                   .OrderByDescending(o => o.Priority)
                                   .ThenBy(o => o.StartDate)
                                   .ToListAsync();
            }
            return new List<SpecialOffer>();
        }

        public async Task<List<SpecialOffer>> GetRecurringOffersAsync(Guid restaurantId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId && o.IsRecurring)
                               .OrderByDescending(o => o.Priority)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetUpcomingOffersAsync(Guid restaurantId, DateTime? fromDate = null)
        {
            var dbSet = await GetDbSetAsync();
            var startDate = fromDate ?? DateTime.Now;
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.StartDate > startDate &&
                                        o.Status == OfferStatus.Scheduled)
                               .OrderBy(o => o.StartDate)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetExpiredOffersAsync(Guid restaurantId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.EndDate < DateTime.Now)
                               .OrderByDescending(o => o.EndDate)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetOffersByPriorityAsync(Guid restaurantId, int minPriority = 1)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.Priority >= minPriority &&
                                        o.IsActive)
                               .OrderByDescending(o => o.Priority)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetOffersForDateAsync(Guid restaurantId, DateTime date)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.StartDate.Date <= date.Date && 
                                        o.EndDate.Date >= date.Date &&
                                        o.IsActive)
                               .OrderByDescending(o => o.Priority)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetOffersForTimeRangeAsync(Guid restaurantId, DateTime startTime, DateTime endTime)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.StartDate <= endTime && 
                                        o.EndDate >= startTime &&
                                        o.IsActive)
                               .OrderByDescending(o => o.Priority)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetOffersForDayOfWeekAsync(Guid restaurantId, DayOfWeek dayOfWeek)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.IsActive &&
                                        (o.ApplicableDays == null || o.ApplicableDays.Contains(dayOfWeek)))
                               .OrderByDescending(o => o.Priority)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> SearchOffersAsync(Guid restaurantId, string searchTerm)
        {
            var dbSet = await GetDbSetAsync();
            var term = searchTerm.ToLower();
            return await dbSet.Where(o => o.RestaurantId == restaurantId &&
                                        (o.Title.ToLower().Contains(term) || 
                                         o.Description.ToLower().Contains(term)))
                               .OrderByDescending(o => o.Priority)
                               .ThenBy(o => o.StartDate)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetOffersByCategoryAsync(Guid restaurantId, string category)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId && 
                                        o.ApplicableCategories.Contains(category))
                               .OrderByDescending(o => o.Priority)
                               .ToListAsync();
        }

        public async Task UpdateOfferUsageAsync(Guid offerId)
        {
            var dbContext = await GetDbContextAsync();
            var dbSet = dbContext.Set<SpecialOffer>();
            var offer = await dbSet.FindAsync(offerId);
            if (offer != null)
            {
                offer.IncrementUsage();
                dbSet.Update(offer);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<SpecialOffer>> GetMostUsedOffersAsync(Guid restaurantId, int count = 10)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(o => o.RestaurantId == restaurantId)
                               .OrderByDescending(o => o.CurrentUses)
                               .Take(count)
                               .ToListAsync();
        }

        public async Task<List<SpecialOffer>> GetOffersNeedingRecurrenceUpdateAsync()
        {
            var dbSet = await GetDbSetAsync();
            var now = DateTime.Now;
            return await dbSet.Where(o => o.IsRecurring && 
                                        o.Status == OfferStatus.Active &&
                                        o.NextOccurrence <= now)
                               .ToListAsync();
        }

        public async Task UpdateNextOccurrenceAsync(Guid offerId, DateTime nextOccurrence)
        {
            var dbContext = await GetDbContextAsync();
            var dbSet = dbContext.Set<SpecialOffer>();
            var offer = await dbSet.FindAsync(offerId);
            if (offer != null)
            {
                offer.NextOccurrence = nextOccurrence;
                offer.CurrentOccurrences++;
                dbSet.Update(offer);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
