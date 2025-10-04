using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class UpdateUserDto
    {
        // UserId is set by the controller from JWT token, not from request body
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[1-9][\d]{0,15}$", ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(2000000, ErrorMessage = "Profile image URL cannot exceed 2MB")]
        public string? ProfileImageUrl { get; set; }

        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string? Role { get; set; } // e.g., "admin", "manager", "operator"
        
        public bool? IsActive { get; set; } // <-- Added for active status
        public bool? EmailConfirmed { get; set; } // <-- Added for email confirmation
        public bool? PhoneNumberConfirmed { get; set; } // <-- Added for phone confirmation
        public bool? IsAdminApproved { get; set; } // <-- Added for admin approval status
        public string? ReviewReason { get; set; } // <-- Added for storing rejection/approval reasons
        public string? ReviewStatus { get; set; } // <-- Added for review status (Pending, Approved, Rejected)
    }
}
