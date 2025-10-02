using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class CreateRestaurantDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public Guid? CategoryId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Range(0, 100)]
        public decimal DeliveryFee { get; set; }

        [Range(0, 1000)]
        public decimal MinimumOrderAmount { get; set; }

        public List<string> Tags { get; set; }

        [Required]
        public AddressDto Address { get; set; }

        public CreateRestaurantDto()
        {
            Tags = new List<string>();
        }
    }

    public class UpdateRestaurantDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public Guid? CategoryId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Range(0, 100)]
        public decimal DeliveryFee { get; set; }

        [Range(0, 1000)]
        public decimal MinimumOrderAmount { get; set; }

        public List<string> Tags { get; set; }

        public AddressDto Address { get; set; }

        public UpdateRestaurantDto()
        {
            Tags = new List<string>();
        }
    }

    public class RestaurantApprovalStatusDto
    {
        public Guid RestaurantId { get; set; }
        public RestaurantApprovalStatus Status { get; set; }
        public string Message { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
