using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    /// <summary>
    /// Category of meals within a specific restaurant (e.g., Starters, Mains, Desserts).
    /// </summary>
    public class MealCategory : FullAuditedAggregateRoot<Guid>
    {
        public MealCategory() { }

        // Constructor with ID parameter for seeding only
        public MealCategory(Guid id) : base(id) { }

        /// <summary>
        /// Restaurant that owns this category
        /// </summary>
        public Guid RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Sorting order within the restaurant (0 = top)
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Navigation: all menu items in this category
        /// </summary>
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
