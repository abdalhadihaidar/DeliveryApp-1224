using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class MealCategoryDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class CreateMealCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;
    }

    public class UpdateMealCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;
    }
}
