using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class RestaurantCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int RestaurantCount { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class CreateRestaurantCategoryDto
    {
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
    }

    public class UpdateRestaurantCategoryDto
    {
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

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
    }

    public class RestaurantCategoryListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int RestaurantCount { get; set; }
    }
}
