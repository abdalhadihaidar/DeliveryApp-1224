using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class CreateSpecialOfferDto
    {
        [Required]
        public Guid RestaurantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Scheduling properties
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public int? MaxOccurrences { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<DayOfWeek>? ApplicableDays { get; set; }

        // Offer properties
        [Range(1, 100)]
        public int? DiscountPercentage { get; set; }

        public double? MinimumOrderAmount { get; set; }

        public bool FreeDelivery { get; set; }

        public bool BuyOneGetOne { get; set; }

        public List<string> ApplicableCategories { get; set; } = new();

        // Status and tracking
        public int Priority { get; set; } = 1;
        public int? MaxUses { get; set; }
        public bool IsActive { get; set; } = true;

        // Validation
        public bool ValidateDates()
        {
            return StartDate < EndDate && StartDate > DateTime.Now.AddDays(-1);
        }
    }

    public class UpdateSpecialOfferDto : CreateSpecialOfferDto
    {
        // Same fields; could extend in future
    }

    public class GetSpecialOfferListDto : PagedAndSortedResultRequestDto
    {
        public Guid? RestaurantId { get; set; }
        public bool? OnlyActive { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
        public bool? IsRecurring { get; set; }
        public int? Priority { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class SpecialOfferScheduleDto
    {
        public Guid OfferId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public int? MaxOccurrences { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<DayOfWeek>? ApplicableDays { get; set; }
    }

    public class RecurrencePatternDto
    {
        public string Type { get; set; } = "Daily"; // Daily, Weekly, Monthly, Yearly
        public int Interval { get; set; } = 1;
        public List<DayOfWeek>? DaysOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxOccurrences { get; set; }
    }
}
