using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class SpecialOfferDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Scheduling properties
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public DateTime? NextOccurrence { get; set; }
        public int? MaxOccurrences { get; set; }
        public int CurrentOccurrences { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<DayOfWeek>? ApplicableDays { get; set; }
        
        // Offer properties
        public int? DiscountPercentage { get; set; }
        public double? MinimumOrderAmount { get; set; }
        public bool FreeDelivery { get; set; }
        public bool BuyOneGetOne { get; set; }
        public List<string> ApplicableCategories { get; set; }
        
        // Status and tracking
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public DateTime? LastUsed { get; set; }
        public int Priority { get; set; }
        
        // Computed properties
        public bool IsValidNow => IsValidAt(DateTime.Now);
        public bool IsExpired => DateTime.Now > EndDate;
        public bool IsUpcoming => DateTime.Now < StartDate;
        public TimeSpan RemainingTime => EndDate > DateTime.Now ? EndDate - DateTime.Now : TimeSpan.Zero;
        
        public SpecialOfferDto()
        {
            ApplicableCategories = new List<string>();
            ApplicableDays = new List<DayOfWeek>();
            Status = "Draft";
            IsActive = true;
            Priority = 1;
            CurrentOccurrences = 0;
            CurrentUses = 0;
        }

        public bool IsValidAt(DateTime dateTime)
        {
            if (!IsActive || Status != "Active")
                return false;

            var date = dateTime.Date;
            var time = dateTime.TimeOfDay;

            // Check date range
            if (date < StartDate.Date || date > EndDate.Date)
                return false;

            // Check daily time range if specified
            if (StartTime.HasValue && EndTime.HasValue)
            {
                if (time < StartTime.Value || time > EndTime.Value)
                    return false;
            }

            // Check applicable days if specified
            if (ApplicableDays != null && ApplicableDays.Count > 0)
            {
                if (!ApplicableDays.Contains(date.DayOfWeek))
                    return false;
            }

            // Check usage limits
            if (MaxUses.HasValue && CurrentUses >= MaxUses.Value)
                return false;

            return true;
        }
    }
}
