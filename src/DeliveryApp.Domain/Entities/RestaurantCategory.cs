using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    public class RestaurantCategory : FullAuditedAggregateRoot<Guid>
    {
        public RestaurantCategory()
        {
            Restaurants = new List<Restaurant>();
        }

        // Constructor with ID parameter for seeding only
        public RestaurantCategory(Guid id) : base(id)
        {
            Restaurants = new List<Restaurant>();
        }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(100)]
        public string? Icon { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        // Navigation property
        public virtual ICollection<Restaurant> Restaurants { get; set; }
    }
}
