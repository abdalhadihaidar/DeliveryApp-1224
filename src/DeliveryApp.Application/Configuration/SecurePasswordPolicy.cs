using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DeliveryApp.Application.Configuration
{
    /// <summary>
    /// Secure password policy configuration
    /// Implements strong password requirements to prevent weak passwords
    /// </summary>
    public static class SecurePasswordPolicy
    {
        public static IServiceCollection AddSecurePasswordPolicy(this IServiceCollection services, SecureAppSettings settings)
        {
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = settings.RequirePasswordComplexity;
                options.Password.RequireLowercase = settings.RequirePasswordComplexity;
                options.Password.RequireNonAlphanumeric = settings.RequirePasswordComplexity;
                options.Password.RequireUppercase = settings.RequirePasswordComplexity;
                options.Password.RequiredLength = settings.MinPasswordLength;
                options.Password.RequiredUniqueChars = 3;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = settings.RequireEmailVerification;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            return services;
        }

        /// <summary>
        /// Validates password strength beyond basic requirements
        /// </summary>
        public static class PasswordValidator
        {
            public static List<string> ValidatePasswordStrength(string password)
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(password))
                {
                    errors.Add("Password cannot be empty");
                    return errors;
                }

                // Length check
                if (password.Length < 8)
                {
                    errors.Add("Password must be at least 8 characters long");
                }

                // Complexity checks
                if (!password.Any(char.IsUpper))
                {
                    errors.Add("Password must contain at least one uppercase letter");
                }

                if (!password.Any(char.IsLower))
                {
                    errors.Add("Password must contain at least one lowercase letter");
                }

                if (!password.Any(char.IsDigit))
                {
                    errors.Add("Password must contain at least one number");
                }

                if (!password.Any(c => !char.IsLetterOrDigit(c)))
                {
                    errors.Add("Password must contain at least one special character");
                }

                // Common password check
                if (IsCommonPassword(password))
                {
                    errors.Add("Password is too common and easily guessable");
                }

                // Sequential character check
                if (HasSequentialCharacters(password))
                {
                    errors.Add("Password cannot contain sequential characters (e.g., abc, 123)");
                }

                // Repeated character check
                if (HasRepeatedCharacters(password))
                {
                    errors.Add("Password cannot contain more than 2 repeated characters in a row");
                }

                return errors;
            }

            private static bool IsCommonPassword(string password)
            {
                var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "password", "123456", "123456789", "qwerty", "abc123", "password123",
                    "admin", "letmein", "welcome", "monkey", "1234567890", "password1",
                    "qwerty123", "dragon", "master", "hello", "freedom", "whatever",
                    "qazwsx", "trustno1", "654321", "jordan23", "harley", "password123",
                    "1234", "robert", "matthew", "jordan", "asshole", "daniel", "helpme",
                    "hunter", "hockey", "ranger", "jordan23", "hunter", "killer", "george",
                    "sexy", "andrew", "charlie", "superman", "asshole", "fuckyou", "dallas",
                    "jessica", "pussy", "pepper", "1234", "michael", "jordan", "tigger",
                    "sunshine", "master", "welcome", "shadow", "ashley", "football",
                    "jesus", "michael", "ninja", "mustang", "password1", "jordan23"
                };

                return commonPasswords.Contains(password);
            }

            private static bool HasSequentialCharacters(string password)
            {
                for (int i = 0; i < password.Length - 2; i++)
                {
                    var char1 = password[i];
                    var char2 = password[i + 1];
                    var char3 = password[i + 2];

                    // Check for sequential letters (abc, bcd, etc.)
                    if (char.IsLetter(char1) && char.IsLetter(char2) && char.IsLetter(char3))
                    {
                        if ((char2 == char1 + 1) && (char3 == char2 + 1))
                        {
                            return true;
                        }
                    }

                    // Check for sequential numbers (123, 234, etc.)
                    if (char.IsDigit(char1) && char.IsDigit(char2) && char.IsDigit(char3))
                    {
                        if ((char2 == char1 + 1) && (char3 == char2 + 1))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private static bool HasRepeatedCharacters(string password)
            {
                for (int i = 0; i < password.Length - 2; i++)
                {
                    if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
