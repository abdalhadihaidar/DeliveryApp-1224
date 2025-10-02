using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DeliveryApp.Application.Configuration
{
    /// <summary>
    /// Secure application settings that read from configuration
    /// This prevents hardcoded secrets in source control
    /// </summary>
    public class SecureAppSettings
    {
        // Database Configuration
        [Required]
        public string DatabaseConnection { get; set; } = string.Empty;

        // JWT Configuration
        [Required]
        public string JwtSecretKey { get; set; } = string.Empty;

        [Required]
        public string JwtIssuer { get; set; } = "https://backend.waselsy.com";

        [Required]
        public string JwtAudience { get; set; } = "https://backend.waselsy.com";

        // OpenIddict Configuration
        [Required]
        public string OpenIddictClientId { get; set; } = "DeliveryApp_App";

        [Required]
        public string OpenIddictClientSecret { get; set; } = string.Empty;

        // String Encryption
        [Required]
        public string StringEncryptionPassPhrase { get; set; } = string.Empty;

        // SendPulse Email Configuration
        public string SendPulseClientId { get; set; } = string.Empty;
        public string SendPulseClientSecret { get; set; } = string.Empty;
        public string SendPulseFromEmail { get; set; } = "noreply@waselsy.com";
        public string SendPulseFromName { get; set; } = "Waseel";

        // Firebase Configuration
        public string FirebaseServiceAccountPath { get; set; } = string.Empty;
        public string FirebaseProjectId { get; set; } = string.Empty;

        // Application URLs
        public string SelfUrl { get; set; } = "https://backend.waselsy.com";
        public string DashboardUrl { get; set; } = "https://dashboard.waselsy.com";

        // Security Settings
        public bool RequireHttps { get; set; } = true;
        public bool RequireEmailVerification { get; set; } = true;
        
        // Password Policy
        public int MinPasswordLength { get; set; } = 12;
        public bool RequirePasswordComplexity { get; set; } = true;

        // Rate Limiting
        public int ApiRateLimitPerMinute { get; set; } = 100;
        public int AuthRateLimitPerMinute { get; set; } = 10;

        /// <summary>
        /// Validates that all required settings are properly configured
        /// </summary>
        public void ValidateConfiguration()
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this);
            
            if (!Validator.TryValidateObject(this, context, validationResults, true))
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                throw new InvalidOperationException($"Configuration validation failed: {errors}");
            }

            // Additional security validations
            if (JwtSecretKey.Length < 32)
                throw new InvalidOperationException("JWT secret key must be at least 32 characters long");

            if (StringEncryptionPassPhrase.Length < 32)
                throw new InvalidOperationException("String encryption passphrase must be at least 32 characters long");

            if (MinPasswordLength < 8)
                throw new InvalidOperationException("Minimum password length must be at least 8 characters");
        }
    }
}
