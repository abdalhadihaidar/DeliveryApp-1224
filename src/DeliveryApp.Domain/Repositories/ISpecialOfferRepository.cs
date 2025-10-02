using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface ISpecialOfferRepository : IRepository<SpecialOffer, Guid>
    {
        Task<List<SpecialOffer>> GetActiveOffersAsync(Guid restaurantId, DateTime? asOf = null);
        Task<List<SpecialOffer>> GetListAsync(int skipCount, int maxResultCount, string sorting, Guid? restaurantId = null);
        
        // Enhanced query methods
        Task<List<SpecialOffer>> GetOffersByStatusAsync(Guid restaurantId, string status);
        Task<List<SpecialOffer>> GetRecurringOffersAsync(Guid restaurantId);
        Task<List<SpecialOffer>> GetUpcomingOffersAsync(Guid restaurantId, DateTime? fromDate = null);
        Task<List<SpecialOffer>> GetExpiredOffersAsync(Guid restaurantId);
        Task<List<SpecialOffer>> GetOffersByPriorityAsync(Guid restaurantId, int minPriority = 1);
        
        // Scheduling methods
        Task<List<SpecialOffer>> GetOffersForDateAsync(Guid restaurantId, DateTime date);
        Task<List<SpecialOffer>> GetOffersForTimeRangeAsync(Guid restaurantId, DateTime startTime, DateTime endTime);
        Task<List<SpecialOffer>> GetOffersForDayOfWeekAsync(Guid restaurantId, DayOfWeek dayOfWeek);
        
        // Search and filtering
        Task<List<SpecialOffer>> SearchOffersAsync(Guid restaurantId, string searchTerm);
        Task<List<SpecialOffer>> GetOffersByCategoryAsync(Guid restaurantId, string category);
        
        // Usage tracking
        Task UpdateOfferUsageAsync(Guid offerId);
        Task<List<SpecialOffer>> GetMostUsedOffersAsync(Guid restaurantId, int count = 10);
        
        // Recurrence management
        Task<List<SpecialOffer>> GetOffersNeedingRecurrenceUpdateAsync();
        Task UpdateNextOccurrenceAsync(Guid offerId, DateTime nextOccurrence);
    }
}
