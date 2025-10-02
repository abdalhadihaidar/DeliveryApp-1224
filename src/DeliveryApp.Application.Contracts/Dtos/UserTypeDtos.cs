using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    // Base user DTO with common properties
    public class UserDto : EntityDto<Guid>
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public UserRole Role { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; } // <-- Added for active status
        public string? ReviewStatus { get; set; } // <-- Added for verification status
        public string? ReviewReason { get; set; } // <-- Added for verification reason
        public DateTime CreationTime { get; set; } // <-- Added for creation time
        public DateTime? LastLoginTime { get; set; } // <-- Added for last login time
        public bool EmailConfirmed { get; set; } // <-- Added for email confirmation
        public bool PhoneNumberConfirmed { get; set; } // <-- Added for phone confirmation
        public string? UserType { get; set; } // <-- Added for user type (string representation for compatibility)
    }

    // Customer specific DTO
    public class CustomerDto : UserDto
    {
        public List<AddressDto> Addresses { get; set; }
        public List<PaymentMethodDto> PaymentMethods { get; set; }
        public List<Guid> FavoriteRestaurantIds { get; set; }
        
        public CustomerDto()
        {
            Role = UserRole.Customer;
            Addresses = new List<AddressDto>();
            PaymentMethods = new List<PaymentMethodDto>();
            FavoriteRestaurantIds = new List<Guid>();
        }
    }

    // Delivery person specific DTO
    public class DeliveryPersonDto : UserDto
    {
        public bool IsAvailable { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public List<OrderSummaryDto> CurrentDeliveries { get; set; }
        
        public DeliveryPersonDto()
        {
            Role = UserRole.Delivery;
            CurrentDeliveries = new List<OrderSummaryDto>();
        }
    }

    // Restaurant owner specific DTO
    public class RestaurantOwnerDto : UserDto
    {
        public List<Guid> ManagedRestaurantIds { get; set; }
        public List<RestaurantSummaryDto> ManagedRestaurants { get; set; }
        public int RestaurantCount { get; set; }
        
        public RestaurantOwnerDto()
        {
            Role = UserRole.RestaurantOwner;
            ManagedRestaurantIds = new List<Guid>();
            ManagedRestaurants = new List<RestaurantSummaryDto>();
        }
    }

    // Detailed restaurant owner DTO for admin view
    public class RestaurantOwnerDetailsDto : RestaurantOwnerDto
    {
        public List<RestaurantSummaryDto> Restaurants { get; set; } = new List<RestaurantSummaryDto>();
        public new int RestaurantCount { get; set; }
    }

    // Using RestaurantSummaryDto from RestaurantDtos.cs

    public class OrderSummaryDto : EntityDto<Guid>
    {
        public string? RestaurantName { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string? DeliveryAddress { get; set; }
    }
}
