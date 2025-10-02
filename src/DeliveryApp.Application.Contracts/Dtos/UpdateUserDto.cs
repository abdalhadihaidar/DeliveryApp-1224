using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class UpdateUserDto
    {
        // UserId is set by the controller from JWT token, not from request body
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        [StringLength(50)]
        public string? Role { get; set; } // e.g., "admin", "manager", "operator"
        
        public bool? IsActive { get; set; } // <-- Added for active status
        public bool? EmailConfirmed { get; set; } // <-- Added for email confirmation
        public bool? PhoneNumberConfirmed { get; set; } // <-- Added for phone confirmation
    }
}
