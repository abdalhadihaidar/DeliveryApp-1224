using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DeliveryApp.Validation
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            
            var phoneNumber = value.ToString();
            
            // Remove all non-digit characters
            var digitsOnly = Regex.Replace(phoneNumber, @"[^\d]", "");
            
            // Check if it's a valid length (7-15 digits)
            return digitsOnly.Length >= 7 && digitsOnly.Length <= 15;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a valid phone number.";
        }
    }

    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            
            var password = value.ToString();
            
            // At least 8 characters, one uppercase, one lowercase, one digit, one special character
            var hasMinLength = password.Length >= 8;
            var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
            var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
            var hasDigit = Regex.IsMatch(password, @"\d");
            var hasSpecialChar = Regex.IsMatch(password, "[!@#$%^&*(),.?\"{}|<>]");
            
            return hasMinLength && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be at least 8 characters long and contain uppercase, lowercase, digit, and special character.";
        }
    }

    public class NoScriptInjectionAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return true;
            
            var input = value.ToString();
            
            // Check for common script injection patterns
            var dangerousPatterns = new[]
            {
                @"<script[^>]*>.*?</script>",
                @"javascript:",
                @"vbscript:",
                @"onload\s*=",
                @"onerror\s*=",
                @"onclick\s*=",
                @"onmouseover\s*=",
                @"<iframe[^>]*>",
                @"<object[^>]*>",
                @"<embed[^>]*>",
                @"<link[^>]*>",
                @"<meta[^>]*>"
            };
            
            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }
            
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} contains potentially dangerous content.";
        }
    }

    public class SqlInjectionProtectionAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return true;
            
            var input = value.ToString().ToLower();
            
            // Check for common SQL injection patterns
            var sqlPatterns = new[]
            {
                @"(\s|^)(select|insert|update|delete|drop|create|alter|exec|execute|union|declare|cast|convert)(\s|$)",
                @"(\s|^)(or|and)(\s|$).*(\s|^)(=|<|>|like)(\s|$)",
                @"'.*'",
                @"--",
                @"/\*.*\*/",
                @"xp_",
                @"sp_"
            };
            
            foreach (var pattern in sqlPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }
            
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} contains potentially dangerous SQL content.";
        }
    }
} 
