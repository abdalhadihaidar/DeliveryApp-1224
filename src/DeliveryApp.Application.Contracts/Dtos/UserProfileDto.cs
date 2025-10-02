using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        public List<AddressDto> Addresses { get; set; }
        
        public List<Guid> FavoriteRestaurants { get; set; }
        
        public List<PaymentMethodDto> PaymentMethods { get; set; }
        
        [StringLength(500)]
        public string ProfileImageUrl { get; set; }
        
        public UserProfileDto()
        {
            Addresses = new List<AddressDto>();
            FavoriteRestaurants = new List<Guid>();
            PaymentMethods = new List<PaymentMethodDto>();
        }
    }
}
