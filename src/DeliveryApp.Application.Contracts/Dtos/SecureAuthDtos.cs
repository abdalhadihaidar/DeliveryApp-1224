using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Validation;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class SecureLoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [NoScriptInjection]
        [SqlInjectionProtection]
        public string Email { get; set; }

        [Required]
        [StrongPassword]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class SecureRegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [NoScriptInjection]
        [SqlInjectionProtection]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [NoScriptInjection]
        [SqlInjectionProtection]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [NoScriptInjection]
        [SqlInjectionProtection]
        public string Email { get; set; }

        [Required]
        [PhoneNumber]
        public string PhoneNumber { get; set; }

        [Required]
        [StrongPassword]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public bool AcceptTerms { get; set; }
    }
    /*
    public class SecureChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StrongPassword]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }
    */
    public class SecureResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string ResetCode { get; set; }

        [Required]
        [StrongPassword]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }

    /// <summary>
    /// DTO for transaction operation results
    /// </summary>
    public class TransactionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? CorrelationId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object? Data { get; set; }
    }

    /// <summary>
    /// DTO for batch operation results
    /// </summary>
    public class BatchOperationResultDto<T>
    {
        public bool Success { get; set; }
        public int TotalItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public List<T> SuccessfulResults { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string? CorrelationId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

