using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface ISpecialOfferAppService : IApplicationService
    {
        // Basic CRUD operations
        Task<PagedResultDto<SpecialOfferDto>> GetListAsync(GetSpecialOfferListDto input);
        Task<SpecialOfferDto> GetAsync(Guid id);
        Task<SpecialOfferDto> CreateAsync(CreateSpecialOfferDto input);
        Task<SpecialOfferDto> UpdateAsync(Guid id, UpdateSpecialOfferDto input);
        Task DeleteAsync(Guid id);
        
        // Enhanced query methods
        Task<List<SpecialOfferDto>> GetOffersByStatusAsync(Guid restaurantId, string status);
        Task<List<SpecialOfferDto>> GetRecurringOffersAsync(Guid restaurantId);
        Task<List<SpecialOfferDto>> GetUpcomingOffersAsync(Guid restaurantId);
        Task<List<SpecialOfferDto>> GetExpiredOffersAsync(Guid restaurantId);
        Task<List<SpecialOfferDto>> GetOffersByPriorityAsync(Guid restaurantId, int minPriority = 1);
        
        // Scheduling methods
        Task<List<SpecialOfferDto>> GetOffersForDateAsync(Guid restaurantId, DateTime date);
        Task<List<SpecialOfferDto>> GetOffersForTimeRangeAsync(Guid restaurantId, DateTime startTime, DateTime endTime);
        Task<List<SpecialOfferDto>> GetOffersForDayOfWeekAsync(Guid restaurantId, DayOfWeek dayOfWeek);
        
        // Search and filtering
        Task<List<SpecialOfferDto>> SearchOffersAsync(Guid restaurantId, string searchTerm);
        Task<List<SpecialOfferDto>> GetOffersByCategoryAsync(Guid restaurantId, string category);
        
        // Status management
        Task<SpecialOfferDto> ActivateOfferAsync(Guid id);
        Task<SpecialOfferDto> DeactivateOfferAsync(Guid id);
        Task<SpecialOfferDto> PauseOfferAsync(Guid id);
        Task<SpecialOfferDto> ResumeOfferAsync(Guid id);
        
        // Scheduling and recurrence
        Task<SpecialOfferDto> ScheduleOfferAsync(Guid id, SpecialOfferScheduleDto scheduleDto);
        Task<SpecialOfferDto> UpdateScheduleAsync(Guid id, SpecialOfferScheduleDto scheduleDto);
        Task ProcessRecurringOffersAsync();
        
        // Usage tracking
        Task UpdateOfferUsageAsync(Guid id);
        Task<List<SpecialOfferDto>> GetMostUsedOffersAsync(Guid restaurantId, int count = 10);
        
        // Restaurant-specific methods
        Task<List<SpecialOfferDto>> GetRestaurantOffersAsync(Guid restaurantId);
        Task<List<SpecialOfferDto>> GetActiveRestaurantOffersAsync(Guid restaurantId);
    }
}
