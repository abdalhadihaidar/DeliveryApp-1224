using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class RestaurantDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
        
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        
        [Range(0, 5)]
        public double Rating { get; set; }
        
        public List<string> Tags { get; set; }
        
        public AddressDto Address { get; set; }
        
        [Range(0, 100)]
        public decimal DeliveryFee { get; set; }
        
        [Range(0, 1000)]
        public decimal MinimumOrderAmount { get; set; }
        
        [Required]
        [StringLength(20)]
        public string DeliveryTime { get; set; }
        
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        
        public List<MenuItemDto> Menu { get; set; }
        
        // Additional properties for admin management
        public bool IsActive { get; set; }
        public decimal? CommissionPercent { get; set; }
        public DateTime CreationTime { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
        
        public RestaurantDto()
        {
            Tags = new List<string>();
            Menu = new List<MenuItemDto>();
        }
    }

    public class RestaurantSummaryDto : EntityDto<Guid>
    {
        
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        
        [Required]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [Range(0, 5)]
        public double Rating { get; set; }
        
        [Range(0, 100)]
        public decimal DeliveryFee { get; set; }
        
        [Required]
        [StringLength(20)]
        public string? DeliveryTime { get; set; }
        public string? Status { get; set; }
    }

    public class MenuItemDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }
        
        [Range(0, 1000)]
        public decimal Price { get; set; }
        
        public Guid? MealCategoryId { get; set; }
        public string? MealCategoryName { get; set; }
        
        public bool IsAvailable { get; set; }
        
        [Range(1, 240)]
        public int PreparationMinutes { get; set; } = 15;
        
        public List<string> Options { get; set; }
        
        public MenuItemDto()
        {
            Options = new List<string>();
        }
    }

    public class GetRestaurantListDto
    {
        public string? Category { get; set; }
        public string? SearchTerm { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
    }

    public class RestaurantSummaryStatisticsDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int TotalOrders { get; set; }
        public double TotalRevenue { get; set; }
        public double AverageOrderValue { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime LastOrderDate { get; set; }
        public int ActiveMenuItems { get; set; }
        public int TotalMenuItems { get; set; }
    }
}
