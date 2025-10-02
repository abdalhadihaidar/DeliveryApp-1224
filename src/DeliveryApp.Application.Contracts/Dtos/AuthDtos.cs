using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Enhanced authentication DTOs supporting email and phone authentication
    /// </summary>
    
    public class RegisterWithEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public string Language { get; set; } = "en";
    }

    public class RegisterWithPhoneDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public string? Email { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public string Language { get; set; } = "en";
    }

    public class LoginWithEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class LoginWithPhoneDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class LoginDto
    {
        [Required]
        public string EmailOrPhone { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class VerifyEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; }
    }

    public class VerifyPhoneDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; }
    }

    public class ResendVerificationCodeDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string VerificationType { get; set; } // Email or Phone
        public string Language { get; set; } = "en";
    }

    public class ResetPasswordDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string ResetCode { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }

    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public class LogoutDto
    {
        public string? RefreshToken { get; set; }
    }

    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserInfoDto? User { get; set; }
        public string? ErrorCode { get; set; }
        public bool RequiresVerification { get; set; } = false;
        public string? VerificationType { get; set; } // Email or Phone
    }

    public class UserInfoDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string PreferredLanguage { get; set; }
    }

    /// <summary>
    /// Authentication configuration settings
    /// </summary>
    public class AuthSettings
    {
        public string JwtSecretKey { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public int AccessTokenExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 30;
        public int VerificationCodeExpiryMinutes { get; set; } = 15;
        public bool RequireEmailVerification { get; set; } = true;
        public bool RequirePhoneVerification { get; set; } = false;
        public bool AllowEmailLogin { get; set; } = true;
        public bool AllowPhoneLogin { get; set; } = true;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 30;
    }
}

