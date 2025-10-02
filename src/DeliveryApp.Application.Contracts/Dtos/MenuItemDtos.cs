using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class CreateMenuItemDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, 1000)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public Guid? MealCategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Range(1, 240)]
        public int PreparationMinutes { get; set; } = 15;

        public List<string> Options { get; set; }

        public CreateMenuItemDto()
        {
            Options = new List<string>();
        }
    }

    public class UpdateMenuItemDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, 1000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        public Guid? MealCategoryId { get; set; }

        public bool IsAvailable { get; set; }

        [Range(1, 240)]
        public int PreparationMinutes { get; set; } = 15;

        public List<string> Options { get; set; }

        public UpdateMenuItemDto()
        {
            Options = new List<string>();
        }
    }
}
