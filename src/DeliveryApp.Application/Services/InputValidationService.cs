using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Logging;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Input validation and sanitization service
    /// Prevents SQL injection, XSS attacks, and other security vulnerabilities
    /// </summary>
    public class InputValidationService
    {
        private readonly ILogger<InputValidationService> _logger;

        public InputValidationService(ILogger<InputValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates and sanitizes text input
        /// </summary>
        public string SanitizeTextInput(string input, int maxLength = 1000)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Trim whitespace
            input = input.Trim();

            // Check length
            if (input.Length > maxLength)
            {
                _logger.LogWarning("Input truncated due to length limit. Original length: {Length}, Max allowed: {MaxLength}", 
                    input.Length, maxLength);
                input = input.Substring(0, maxLength);
            }

            // HTML encode to prevent XSS
            input = HttpUtility.HtmlEncode(input);

            // Remove potentially dangerous characters
            input = RemoveDangerousCharacters(input);

            return input;
        }

        /// <summary>
        /// Validates email address format
        /// </summary>
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return emailRegex.IsMatch(email) && email.Length <= 254;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email: {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Validates phone number format
        /// </summary>
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove all non-digit characters
            var digitsOnly = Regex.Replace(phoneNumber, @"\D", "");

            // Check if it's a valid length (7-15 digits)
            return digitsOnly.Length >= 7 && digitsOnly.Length <= 15;
        }

        /// <summary>
        /// Validates GUID format
        /// </summary>
        public bool IsValidGuid(string guidString)
        {
            if (string.IsNullOrWhiteSpace(guidString))
                return false;

            return Guid.TryParse(guidString, out _);
        }

        /// <summary>
        /// Validates search term for SQL injection prevention
        /// </summary>
        public string SanitizeSearchTerm(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return string.Empty;

            // Remove SQL injection patterns
            var sqlInjectionPatterns = new[]
            {
                @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|UNION|SCRIPT)\b)",
                @"(--|/\*|\*/)",
                @"(\b(OR|AND)\s+\d+\s*=\s*\d+)",
                @"(\b(OR|AND)\s+'.*'\s*=\s*'.*')",
                @"(;|\||&)",
                @"(\b(WAITFOR|DELAY)\b)",
                @"(\b(CHAR|ASCII|SUBSTRING|LEN)\b)"
            };

            foreach (var pattern in sqlInjectionPatterns)
            {
                searchTerm = Regex.Replace(searchTerm, pattern, "", RegexOptions.IgnoreCase);
            }

            // Remove special characters that could be dangerous
            searchTerm = Regex.Replace(searchTerm, @"[<>""'%;()&+]", "");

            // Limit length
            if (searchTerm.Length > 100)
            {
                searchTerm = searchTerm.Substring(0, 100);
            }

            return searchTerm.Trim();
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        public List<string> ValidatePassword(string password)
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

            // Check for common passwords
            if (IsCommonPassword(password))
            {
                errors.Add("Password is too common and easily guessable");
            }

            return errors;
        }

        /// <summary>
        /// Validates file upload
        /// </summary>
        public bool IsValidFileUpload(string fileName, long fileSize, string[] allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Check file size (max 10MB)
            if (fileSize > 10 * 1024 * 1024)
            {
                _logger.LogWarning("File upload rejected due to size limit. File: {FileName}, Size: {FileSize}", 
                    fileName, fileSize);
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("File upload rejected due to invalid extension. File: {FileName}, Extension: {Extension}", 
                    fileName, extension);
                return false;
            }

            // Check for dangerous file names
            var dangerousPatterns = new[] { @"\.\.", @"\/", @"\\", @"<", @">", @"""", @"'", @"%" };
            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(fileName, pattern))
                {
                    _logger.LogWarning("File upload rejected due to dangerous characters. File: {FileName}", fileName);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates URL format
        /// </summary>
        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private string RemoveDangerousCharacters(string input)
        {
            // Remove null bytes
            input = input.Replace("\0", "");

            // Remove control characters except newline and carriage return
            input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

            return input;
        }

        private bool IsCommonPassword(string password)
        {
            var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "password", "123456", "123456789", "qwerty", "abc123", "password123",
                "admin", "letmein", "welcome", "monkey", "1234567890", "password1",
                "qwerty123", "dragon", "master", "hello", "freedom", "whatever",
                "qazwsx", "trustno1", "654321", "jordan23", "harley", "password123"
            };

            return commonPasswords.Contains(password);
        }
    }
}
