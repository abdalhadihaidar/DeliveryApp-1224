using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    public class Advertisement : FullAuditedAggregateRoot<Guid>
    {
        public Advertisement() { }
        
        public Advertisement(Guid id) : base(id) { }
        
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

        public int? ClickCount { get; set; } = 0;

        public int? ViewCount { get; set; } = 0;

        // For tracking which restaurant or entity this ad is for (optional)
        public Guid? RestaurantId { get; set; }
        public virtual Restaurant? Restaurant { get; set; }

        // For tracking which user created this ad (admin) - optional
        public Guid? CreatedById { get; set; }
        public virtual AppUser? CreatedBy { get; set; }
    }
} 
