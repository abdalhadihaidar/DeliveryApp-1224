using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    public class SpecialOffer : FullAuditedAggregateRoot<Guid>
    {
        public Guid RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Scheduling properties
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; } // JSON string for recurrence rules
        public DateTime? NextOccurrence { get; set; }
        public int? MaxOccurrences { get; set; }
        public int CurrentOccurrences { get; set; }
        
        // Offer properties
        public int? DiscountPercentage { get; set; }
        public double? MinimumOrderAmount { get; set; }
        public bool FreeDelivery { get; set; }
        public bool BuyOneGetOne { get; set; }
        public List<string> ApplicableCategories { get; set; }
        
        // Status and tracking
        public OfferStatus Status { get; set; }
        public bool IsActive { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public DateTime? LastUsed { get; set; }
        
        // Priority and scheduling
        public int Priority { get; set; } // Higher number = higher priority
        public TimeSpan? StartTime { get; set; } // Daily start time
        public TimeSpan? EndTime { get; set; } // Daily end time
        public List<DayOfWeek>? ApplicableDays { get; set; } // Days of week when offer is valid

        public SpecialOffer()
        {
            ApplicableCategories = new List<string>();
            ApplicableDays = new List<DayOfWeek>();
            Status = OfferStatus.Draft;
            IsActive = true;
            Priority = 1;
            CurrentOccurrences = 0;
            CurrentUses = 0;
        }

        public SpecialOffer(Guid id) : base(id)
        {
            ApplicableCategories = new List<string>();
            ApplicableDays = new List<DayOfWeek>();
            Status = OfferStatus.Draft;
            IsActive = true;
            Priority = 1;
            CurrentOccurrences = 0;
            CurrentUses = 0;
        }

        // Business logic methods
        public bool IsValidAt(DateTime dateTime)
        {
            if (!IsActive || Status != OfferStatus.Active)
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

        public void Activate()
        {
            Status = OfferStatus.Active;
            IsActive = true;
        }

        public void Deactivate()
        {
            Status = OfferStatus.Inactive;
            IsActive = false;
        }

        public void Pause()
        {
            Status = OfferStatus.Paused;
        }

        public void Resume()
        {
            if (DateTime.Now >= StartDate && DateTime.Now <= EndDate)
            {
                Status = OfferStatus.Active;
            }
        }

        public void IncrementUsage()
        {
            CurrentUses++;
            LastUsed = DateTime.Now;
        }

        public bool CanBeUsed()
        {
            return IsActive && 
                   Status == OfferStatus.Active && 
                   (!MaxUses.HasValue || CurrentUses < MaxUses.Value) &&
                   DateTime.Now >= StartDate && 
                   DateTime.Now <= EndDate;
        }
    }

    public enum OfferStatus
    {
        Draft = 0,
        Active = 1,
        Paused = 2,
        Inactive = 3,
        Expired = 4,
        Scheduled = 5
    }
}
