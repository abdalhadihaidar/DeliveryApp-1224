using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using System.Linq;

namespace DeliveryApp.Application.Services
{
    public class OfferSchedulingService : ApplicationService, IOfferSchedulingService
    {
        private readonly ISpecialOfferRepository _offerRepository;
        private readonly ILogger<OfferSchedulingService> _logger;

        public OfferSchedulingService(
            ISpecialOfferRepository offerRepository,
            ILogger<OfferSchedulingService> logger)
        {
            _offerRepository = offerRepository;
            _logger = logger;
        }

        public async Task<SpecialOfferDto> CreateRecurringOfferAsync(CreateSpecialOfferDto input, RecurrencePatternDto recurrencePattern)
        {
            try
            {
                // Validate recurrence pattern
                if (!ValidateRecurrencePattern(recurrencePattern))
                {
                    throw new ArgumentException("Invalid recurrence pattern");
                }

                // Create the offer
                var offer = new SpecialOffer(GuidGenerator.Create())
                {
                    RestaurantId = input.RestaurantId,
                    Title = input.Title,
                    Description = input.Description ?? string.Empty,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    IsRecurring = true,
                    RecurrencePattern = JsonSerializer.Serialize(recurrencePattern),
                    MaxOccurrences = recurrencePattern.MaxOccurrences,
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
                    Status = input.IsActive ? OfferStatus.Active : OfferStatus.Scheduled,
                    NextOccurrence = input.StartDate
                };

                await _offerRepository.InsertAsync(offer);
                return ObjectMapper.Map<SpecialOffer, SpecialOfferDto>(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring offer");
                throw;
            }
        }

        public async Task<List<DateTime>> GetUpcomingOccurrencesAsync(Guid offerId, int count = 10)
        {
            try
            {
                var offer = await _offerRepository.GetAsync(offerId);
                if (!offer.IsRecurring || string.IsNullOrEmpty(offer.RecurrencePattern))
                {
                    return new List<DateTime> { offer.StartDate };
                }

                var pattern = JsonSerializer.Deserialize<RecurrencePatternDto>(offer.RecurrencePattern);
                if (pattern == null) return new List<DateTime>();

                var occurrences = new List<DateTime>();
                var currentDate = offer.NextOccurrence ?? offer.StartDate;
                var occurrenceCount = 0;

                while (occurrenceCount < count && currentDate <= offer.EndDate)
                {
                    if (currentDate >= DateTime.Now)
                    {
                        occurrences.Add(currentDate);
                        occurrenceCount++;
                    }

                    currentDate = CalculateNextOccurrence(currentDate, pattern);
                }

                return occurrences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming occurrences for offer {OfferId}", offerId);
                return new List<DateTime>();
            }
        }

        public async Task<bool> IsOfferValidAtAsync(Guid offerId, DateTime dateTime)
        {
            try
            {
                var offer = await _offerRepository.GetAsync(offerId);
                return offer.IsValidAt(dateTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking offer validity for offer {OfferId}", offerId);
                return false;
            }
        }

        public async Task<List<SpecialOfferDto>> GetConflictingOffersAsync(Guid restaurantId, DateTime startDate, DateTime endDate, Guid? excludeOfferId = null)
        {
            try
            {
                var offers = await _offerRepository.GetOffersForTimeRangeAsync(restaurantId, startDate, endDate);
                
                // Filter out the excluded offer and inactive offers
                var conflictingOffers = offers.Where(o => 
                    o.Id != excludeOfferId && 
                    o.IsActive && 
                    o.Status == OfferStatus.Active &&
                    o.IsValidAt(startDate)).ToList();

                return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(conflictingOffers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conflicting offers for restaurant {RestaurantId}", restaurantId);
                return new List<SpecialOfferDto>();
            }
        }

        public async Task<bool> ValidateOfferScheduleAsync(CreateSpecialOfferDto input)
        {
            try
            {
                // Check for conflicting offers
                var conflictingOffers = await GetConflictingOffersAsync(
                    input.RestaurantId, 
                    input.StartDate, 
                    input.EndDate);

                // Check if there are too many active offers at the same time
                if (conflictingOffers.Count >= 5) // Maximum 5 active offers at the same time
                {
                    return false;
                }

                // Check for overlapping time-based offers
                if (input.StartTime.HasValue && input.EndTime.HasValue)
                {
                    var timeConflicts = conflictingOffers.Where(o => 
                        o.StartTime.HasValue && o.EndTime.HasValue &&
                        ((input.StartTime.Value >= o.StartTime.Value && input.StartTime.Value < o.EndTime.Value) ||
                         (input.EndTime.Value > o.StartTime.Value && input.EndTime.Value <= o.EndTime.Value))).ToList();

                    if (timeConflicts.Any())
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating offer schedule");
                return false;
            }
        }

        private bool ValidateRecurrencePattern(RecurrencePatternDto pattern)
        {
            if (pattern == null) return false;

            // Validate interval
            if (pattern.Interval <= 0) return false;

            // Validate type
            var validTypes = new[] { "daily", "weekly", "monthly", "yearly" };
            if (!validTypes.Contains(pattern.Type.ToLower())) return false;

            // Validate weekly pattern
            if (pattern.Type.ToLower() == "weekly" && 
                (pattern.DaysOfWeek == null || pattern.DaysOfWeek.Count == 0))
            {
                return false;
            }

            // Validate monthly pattern
            if (pattern.Type.ToLower() == "monthly" && 
                (!pattern.DayOfMonth.HasValue || pattern.DayOfMonth.Value < 1 || pattern.DayOfMonth.Value > 31))
            {
                return false;
            }

            return true;
        }

        private DateTime CalculateNextOccurrence(DateTime currentDate, RecurrencePatternDto pattern)
        {
            return pattern.Type.ToLower() switch
            {
                "daily" => currentDate.AddDays(pattern.Interval),
                "weekly" => currentDate.AddDays(7 * pattern.Interval),
                "monthly" => currentDate.AddMonths(pattern.Interval),
                "yearly" => currentDate.AddYears(pattern.Interval),
                _ => currentDate.AddDays(1)
            };
        }
    }

    public interface IOfferSchedulingService : IApplicationService
    {
        Task<SpecialOfferDto> CreateRecurringOfferAsync(CreateSpecialOfferDto input, RecurrencePatternDto recurrencePattern);
        Task<List<DateTime>> GetUpcomingOccurrencesAsync(Guid offerId, int count = 10);
        Task<bool> IsOfferValidAtAsync(Guid offerId, DateTime dateTime);
        Task<List<SpecialOfferDto>> GetConflictingOffersAsync(Guid restaurantId, DateTime startDate, DateTime endDate, Guid? excludeOfferId = null);
        Task<bool> ValidateOfferScheduleAsync(CreateSpecialOfferDto input);
    }
}
