using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Application.Services
{
    public class SpecialOfferAppService : ApplicationService, ISpecialOfferAppService
    {
        private readonly ISpecialOfferRepository _offerRepository;
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<SpecialOfferAppService> _logger;

        public SpecialOfferAppService(
            ISpecialOfferRepository offerRepository,
            IRepository<Restaurant, Guid> restaurantRepository,
            ICurrentUser currentUser,
            ILogger<SpecialOfferAppService> logger)
        {
            _offerRepository = offerRepository;
            _restaurantRepository = restaurantRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<PagedResultDto<SpecialOfferDto>> GetListAsync(GetSpecialOfferListDto input)
        {
            // Optimize: Apply all filters at database level instead of in-memory filtering
            var queryable = await _offerRepository.GetQueryableAsync();
            var query = queryable.AsQueryable();
            
            // Apply restaurant filter first
            if (input.RestaurantId.HasValue)
            {
                query = query.Where(o => o.RestaurantId == input.RestaurantId.Value);
            }
            
            // Apply additional filters efficiently with single query execution
            var date = input.Date ?? DateTime.Now;
            query = query.Where(o => 
                (!input.OnlyActive.HasValue || !input.OnlyActive.Value || o.IsValidAt(date)) &&
                (string.IsNullOrEmpty(input.Status) || o.Status.ToString() == input.Status) &&
                (!input.IsRecurring.HasValue || o.IsRecurring == input.IsRecurring.Value) &&
                (!input.Priority.HasValue || o.Priority >= input.Priority.Value) &&
                (string.IsNullOrEmpty(input.SearchTerm) || 
                 o.Title.Contains(input.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                 o.Description.Contains(input.SearchTerm, StringComparison.OrdinalIgnoreCase))
            );
            
            // Apply sorting
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = input.Sorting.ToLower() switch
                {
                    "creationtime" => query.OrderByDescending(o => o.CreationTime),
                    "title" => query.OrderBy(o => o.Title),
                    "priority" => query.OrderByDescending(o => o.Priority),
                    _ => query.OrderByDescending(o => o.CreationTime)
                };
            }
            else
            {
                query = query.OrderByDescending(o => o.CreationTime);
            }
            
            var total = await query.CountAsync();
            var offers = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync(); // Single ToList() call
            
            var dtoList = ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
            return new PagedResultDto<SpecialOfferDto>(total, dtoList);
        }

        [AllowAnonymous]
        public async Task<SpecialOfferDto> GetAsync(Guid id)
        {
            var offer = await _offerRepository.GetAsync(id);
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        [Authorize]
        public async Task<SpecialOfferDto> CreateAsync(CreateSpecialOfferDto input)
        {
            await ValidateOwnershipAsync(input.RestaurantId);
            
            if (!input.ValidateDates())
            {
                throw new UserFriendlyException("Invalid date range. Start date must be before end date and start date cannot be in the past.");
            }
            
            var offer = new SpecialOffer(GuidGenerator.Create())
            {
                RestaurantId = input.RestaurantId,
                Title = input.Title,
                Description = input.Description ?? string.Empty,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                IsRecurring = input.IsRecurring,
                RecurrencePattern = input.RecurrencePattern,
                MaxOccurrences = input.MaxOccurrences,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                ApplicableDays = input.ApplicableDays ?? new List<DayOfWeek>(),
                DiscountPercentage = input.DiscountPercentage,
                MinimumOrderAmount = input.MinimumOrderAmount,
                FreeDelivery = input.FreeDelivery,
                BuyOneGetOne = input.BuyOneGetOne,
                ApplicableCategories = input.ApplicableCategories ?? new List<string>(),
                Priority = input.Priority,
                MaxUses = input.MaxUses,
                IsActive = input.IsActive,
                Status = input.IsActive ? OfferStatus.Active : OfferStatus.Draft
            };
            
            if (offer.IsRecurring)
            {
                offer.NextOccurrence = offer.StartDate;
            }
            
            await _offerRepository.InsertAsync(offer);
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        [Authorize]
        public async Task<SpecialOfferDto> UpdateAsync(Guid id, UpdateSpecialOfferDto input)
        {
            var offer = await _offerRepository.GetAsync(id);
            await ValidateOwnershipAsync(offer.RestaurantId);
            
            if (!input.ValidateDates())
            {
                throw new UserFriendlyException("Invalid date range. Start date must be before end date and start date cannot be in the past.");
            }
            
            ObjectMapper.Map(input, offer);
            
            if (offer.IsRecurring && offer.NextOccurrence == null)
            {
                offer.NextOccurrence = offer.StartDate;
            }
            
            await _offerRepository.UpdateAsync(offer);
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        [Authorize]
        public async Task DeleteAsync(Guid id)
        {
            var offer = await _offerRepository.GetAsync(id);
            await ValidateOwnershipAsync(offer.RestaurantId);
            await _offerRepository.DeleteAsync(id);
        }

        // Enhanced query methods
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetOffersByStatusAsync(Guid restaurantId, string status)
        {
            var offers = await _offerRepository.GetOffersByStatusAsync(restaurantId, status);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetRecurringOffersAsync(Guid restaurantId)
        {
            var offers = await _offerRepository.GetRecurringOffersAsync(restaurantId);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetUpcomingOffersAsync(Guid restaurantId)
        {
            var offers = await _offerRepository.GetUpcomingOffersAsync(restaurantId);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetExpiredOffersAsync(Guid restaurantId)
        {
            var offers = await _offerRepository.GetExpiredOffersAsync(restaurantId);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetOffersByPriorityAsync(Guid restaurantId, int minPriority = 1)
        {
            var offers = await _offerRepository.GetOffersByPriorityAsync(restaurantId, minPriority);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        // Scheduling methods
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetOffersForDateAsync(Guid restaurantId, DateTime date)
        {
            var offers = await _offerRepository.GetOffersForDateAsync(restaurantId, date);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetOffersForTimeRangeAsync(Guid restaurantId, DateTime startTime, DateTime endTime)
        {
            var offers = await _offerRepository.GetOffersForTimeRangeAsync(restaurantId, startTime, endTime);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetOffersForDayOfWeekAsync(Guid restaurantId, DayOfWeek dayOfWeek)
        {
            var offers = await _offerRepository.GetOffersForDayOfWeekAsync(restaurantId, dayOfWeek);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        // Search and filtering
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> SearchOffersAsync(Guid restaurantId, string searchTerm)
        {
            var offers = await _offerRepository.SearchOffersAsync(restaurantId, searchTerm);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetOffersByCategoryAsync(Guid restaurantId, string category)
        {
            var offers = await _offerRepository.GetOffersByCategoryAsync(restaurantId, category);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        // Status management
        [Authorize]
        public async Task<SpecialOfferDto> ActivateOfferAsync(Guid id)
        {
            var offer = await _offerRepository.GetAsync(id);
            await ValidateOwnershipAsync(offer.RestaurantId);
            
            offer.Activate();
            await _offerRepository.UpdateAsync(offer);
            
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        [Authorize]
        public async Task<SpecialOfferDto> DeactivateOfferAsync(Guid id)
        {
            var offer = await _offerRepository.GetAsync(id);
            await ValidateOwnershipAsync(offer.RestaurantId);
            
            offer.Deactivate();
            await _offerRepository.UpdateAsync(offer);
            
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        [Authorize]
        public async Task<SpecialOfferDto> PauseOfferAsync(Guid id)
        {
            var offer = await _offerRepository.GetAsync(id);
            await ValidateOwnershipAsync(offer.RestaurantId);
            
            offer.Pause();
            await _offerRepository.UpdateAsync(offer);
            
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        [Authorize]
        public async Task<SpecialOfferDto> ResumeOfferAsync(Guid id)
        {
            var offer = await _offerRepository.GetAsync(id);
            await ValidateOwnershipAsync(offer.RestaurantId);
            
            offer.Resume();
            await _offerRepository.UpdateAsync(offer);
            
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        // Scheduling and recurrence
        [Authorize]
        public async Task<SpecialOfferDto> ScheduleOfferAsync(Guid id, SpecialOfferScheduleDto scheduleDto)
        {
            var offer = await _offerRepository.GetAsync(id);
            await ValidateOwnershipAsync(offer.RestaurantId);
            
            offer.StartDate = scheduleDto.StartDate;
            offer.EndDate = scheduleDto.EndDate;
            offer.IsRecurring = scheduleDto.IsRecurring;
            offer.RecurrencePattern = scheduleDto.RecurrencePattern;
            offer.MaxOccurrences = scheduleDto.MaxOccurrences;
            offer.StartTime = scheduleDto.StartTime;
            offer.EndTime = scheduleDto.EndTime;
            offer.ApplicableDays = scheduleDto.ApplicableDays ?? new List<DayOfWeek>();
            offer.Status = OfferStatus.Scheduled;
            
            if (offer.IsRecurring)
            {
                offer.NextOccurrence = offer.StartDate;
            }
            
            await _offerRepository.UpdateAsync(offer);
            return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
        }

        [Authorize]
        public async Task<SpecialOfferDto> UpdateScheduleAsync(Guid id, SpecialOfferScheduleDto scheduleDto)
        {
            return await ScheduleOfferAsync(id, scheduleDto);
        }

        [Authorize]
        public async Task ProcessRecurringOffersAsync()
        {
            try
            {
                var offersNeedingUpdate = await _offerRepository.GetOffersNeedingRecurrenceUpdateAsync();
                
                foreach (var offer in offersNeedingUpdate)
                {
                    if (offer.RecurrencePattern != null)
                    {
                        var nextOccurrence = CalculateNextOccurrence(offer);
                        if (nextOccurrence.HasValue)
                        {
                            await _offerRepository.UpdateNextOccurrenceAsync(offer.Id, nextOccurrence.Value);
                        }
                    }
                }
                
                _logger.LogInformation("Processed {Count} recurring offers", offersNeedingUpdate.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring offers");
                throw;
            }
        }

        // Usage tracking
        [Authorize]
        public async Task UpdateOfferUsageAsync(Guid id)
        {
            await _offerRepository.UpdateOfferUsageAsync(id);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetMostUsedOffersAsync(Guid restaurantId, int count = 10)
        {
            var offers = await _offerRepository.GetMostUsedOffersAsync(restaurantId, count);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        // Restaurant-specific methods
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetRestaurantOffersAsync(Guid restaurantId)
        {
            var offers = await _offerRepository.GetListAsync(0, 1000, "CreationTime", restaurantId);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetActiveRestaurantOffersAsync(Guid restaurantId)
        {
            var offers = await _offerRepository.GetActiveOffersAsync(restaurantId);
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        // Helper methods
        private async Task ValidateOwnershipAsync(Guid restaurantId)
        {
            // Admins bypass
            if (_currentUser.IsInRole("admin") || _currentUser.IsInRole("manager")) return;
            
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            var currentUserId = _currentUser.GetId();
            
            if (restaurant.OwnerId != currentUserId)
            {
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Not authorized to modify offers for this restaurant.");
            }
        }

        private DateTime? CalculateNextOccurrence(SpecialOffer offer)
        {
            if (string.IsNullOrEmpty(offer.RecurrencePattern))
                return null;

            try
            {
                var pattern = JsonSerializer.Deserialize<RecurrencePatternDto>(offer.RecurrencePattern);
                if (pattern == null) return null;

                var currentDate = offer.NextOccurrence ?? offer.StartDate;
                var nextDate = currentDate;

                switch (pattern.Type.ToLower())
                {
                    case "daily":
                        nextDate = currentDate.AddDays(pattern.Interval);
                        break;
                    case "weekly":
                        nextDate = currentDate.AddDays(7 * pattern.Interval);
                        break;
                    case "monthly":
                        nextDate = currentDate.AddMonths(pattern.Interval);
                        break;
                    case "yearly":
                        nextDate = currentDate.AddYears(pattern.Interval);
                        break;
                }

                // Check if we've exceeded max occurrences
                if (offer.MaxOccurrences.HasValue && 
                    offer.CurrentOccurrences >= offer.MaxOccurrences.Value)
                {
                    return null;
                }

                // Check if we've exceeded end date
                if (pattern.EndDate.HasValue && nextDate > pattern.EndDate.Value)
                {
                    return null;
                }

                return nextDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next occurrence for offer {OfferId}", offer.Id);
                return null;
            }
        }
    }
}
