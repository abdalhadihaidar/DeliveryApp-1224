using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class AdRequestDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string LinkUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Priority { get; set; }
        public string? TargetAudience { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReviewReason { get; set; }
        public Guid? ReviewedById { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? AdvertisementId { get; set; }
        public decimal? Budget { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateAdRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string LinkUrl { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 10)]
        public int Priority { get; set; } = 1;

        [StringLength(50)]
        public string? TargetAudience { get; set; }

        [StringLength(50)]
        public string? Location { get; set; }

        [Range(0, 100000)]
        public decimal? Budget { get; set; }

        public bool ValidateDates()
        {
            return StartDate >= DateTime.UtcNow.Date && EndDate > StartDate;
        }
    }

    public class UpdateAdRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string LinkUrl { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 10)]
        public int Priority { get; set; } = 1;

        [StringLength(50)]
        public string? TargetAudience { get; set; }

        [StringLength(50)]
        public string? Location { get; set; }

        [Range(0, 100000)]
        public decimal? Budget { get; set; }

        public bool ValidateDates()
        {
            return StartDate >= DateTime.UtcNow.Date && EndDate > StartDate;
        }
    }

    public class ReviewAdRequestDto
    {
        [Required]
        public bool Approve { get; set; }

        [StringLength(1000)]
        public string? ReviewReason { get; set; }
    }

    public class GetAdRequestListDto : PagedAndSortedResultRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public Guid? RestaurantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
