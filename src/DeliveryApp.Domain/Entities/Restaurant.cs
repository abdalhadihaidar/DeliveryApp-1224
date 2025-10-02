using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    public class Restaurant : FullAuditedAggregateRoot<Guid>
    {
        public Restaurant() 
        {
            Tags = new List<string>();
            Menu = new List<MenuItem>();
        }
        
        // Constructor with ID parameter for seeding only
        public Restaurant(Guid id) : base(id) 
        {
            Tags = new List<string>();
            Menu = new List<MenuItem>();
        }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        // Restaurant Category relationship
        public Guid? CategoryId { get; set; }
        public virtual RestaurantCategory? Category { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        [Required]
        [StringLength(20)]
        public string DeliveryTime { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal DeliveryFee { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<string> Tags { get; set; }
        public virtual ICollection<MenuItem> Menu { get; set; }
        public virtual Address? Address { get; set; }

        // Restaurant owner
        public Guid OwnerId { get; set; }
        public virtual AppUser Owner { get; set; } = null!;

        // Minimum order amount
        [Range(0, 1000)]
        public decimal MinimumOrderAmount { get; set; }

        // Commission rate for restaurant (e.g., 0.15 = 15%)
        public decimal? CommissionRate { get; set; }

        // Commission percentage for restaurant (e.g., 15 = 15%) - alias for CommissionRate
        public decimal? CommissionPercent 
        { 
            get => CommissionRate.HasValue ? CommissionRate.Value * 100 : null;
            set => CommissionRate = value.HasValue ? value.Value / 100 : null;
        }

        // Restaurant activation status
        public bool IsActive { get; set; } = true;

        // Stripe connected account id (if the restaurant has onboarded to Stripe Connect)
        public string? StripeConnectedAccountId { get; set; }

        // Default constructor removed to avoid duplication
    }

    public class MenuItem : FullAuditedEntity<Guid>
    {
        // Constructor with ID parameter for seeding only
        public MenuItem(Guid id) : base(id) 
        {
            Options = new List<string>();
        }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public Guid? MealCategoryId { get; set; }
        public virtual MealCategory? MealCategory { get; set; }

        public bool IsAvailable { get; set; }
        public bool IsPopular { get; set; }

        [Range(1, 240)]
        public int PreparationMinutes { get; set; } = 15;

        public virtual ICollection<string> Options { get; set; }
        public Guid RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; } = null!;

        // Default constructor for EF Core
        public MenuItem()
        {
            Options = new List<string>();
        }
    }

    public class Address : FullAuditedEntity<Guid>
    {
        public Address() { }
        
        // Constructor with ID parameter for seeding only
        public Address(Guid id) : base(id) { }
        
        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [StringLength(500)]
        public string? FullAddress { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // For user addresses
        public string? Title { get; set; }
        public bool IsDefault { get; set; }
        public Guid? UserId { get; set; }
        public virtual AppUser? User { get; set; }

        // For restaurant addresses
        public Guid? RestaurantId { get; set; }
        public virtual Restaurant? Restaurant { get; set; }
    }
}
