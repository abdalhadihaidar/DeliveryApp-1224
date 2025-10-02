using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// DTO for updating user profile securely
    /// </summary>
    public class SecureUpdateUserProfileDto
    {
        [StringLength(50, MinimumLength = 2)]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string? LastName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(10)]
        public string? Language { get; set; }
    }

    /// <summary>
    /// DTO for changing password securely
    /// </summary>
    public class SecureChangePasswordDto
    {
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for secure account operations
    /// </summary>
    public class SecureAccountOperationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }
    }

}
