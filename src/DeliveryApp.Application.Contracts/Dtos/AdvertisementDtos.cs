using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class AdvertisementDto
    {
        public Guid Id { get; set; }
        
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
        
        public bool IsActive { get; set; }
        
        [Range(1, 10)]
        public int Priority { get; set; }
        
        [StringLength(50)]
        public string? TargetAudience { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }
        
        public int? ClickCount { get; set; }
        
        public int? ViewCount { get; set; }
        
        public Guid? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        
        public Guid? CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateAdvertisementDto
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
        
        public bool IsActive { get; set; } = true;
        
        [Range(1, 10)]
        public int Priority { get; set; } = 1;
        
        [StringLength(50)]
        public string? TargetAudience { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }
        
        public Guid? RestaurantId { get; set; }
    }

    public class UpdateAdvertisementDto
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
        
        public bool IsActive { get; set; }
        
        [Range(1, 10)]
        public int Priority { get; set; }
        
        [StringLength(50)]
        public string? TargetAudience { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }
        
        public Guid? RestaurantId { get; set; }
    }

    public class GetAdvertisementListDto : PagedAndSortedResultRequestDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? Location { get; set; }
        public Guid? RestaurantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AdvertisementSummaryDto : EntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public int? ClickCount { get; set; }
        public int? ViewCount { get; set; }
    }
} 
