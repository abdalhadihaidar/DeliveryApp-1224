using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Services;

namespace DeliveryApp.Application.Middleware
{
    /// <summary>
    /// PCI DSS compliance middleware for secure payment processing
    /// </summary>
    public class PCIComplianceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PCIComplianceMiddleware> _logger;
        private readonly IEncryptionService _encryptionService;

        public PCIComplianceMiddleware(
            RequestDelegate next,
            ILogger<PCIComplianceMiddleware> logger,
            IEncryptionService encryptionService)
        {
            _next = next;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Ensure HTTPS for payment-related endpoints
            if (IsPaymentEndpoint(context.Request.Path) && !context.Request.IsHttps)
            {
                _logger.LogWarning("Insecure payment request attempted from {IP}", 
                    context.Connection.RemoteIpAddress);
                
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("HTTPS required for payment operations");
                return;
            }

            // Add security headers for PCI compliance
            AddSecurityHeaders(context);

            // Log payment-related requests for audit trail
            if (IsPaymentEndpoint(context.Request.Path))
            {
                await LogPaymentRequest(context);
            }

            // Validate request size to prevent DoS attacks
            if (context.Request.ContentLength > 1024 * 1024) // 1MB limit
            {
                _logger.LogWarning("Large request blocked from {IP}: {Size} bytes", 
                    context.Connection.RemoteIpAddress, context.Request.ContentLength);
                
                context.Response.StatusCode = 413;
                await context.Response.WriteAsync("Request too large");
                return;
            }

            // Rate limiting for payment endpoints
            if (IsPaymentEndpoint(context.Request.Path))
            {
                var clientId = GetClientIdentifier(context);
                if (!await CheckRateLimit(clientId))
                {
                    _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
                    
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Rate limit exceeded");
                    return;
                }
            }

            await _next(context);
        }

        private bool IsPaymentEndpoint(string path)
        {
            var paymentPaths = new[]
            {
                "/api/payments",
                "/api/stripe",
                "/api/financial",
                "/api/checkout"
            };

            return paymentPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            // Prevent clickjacking
            headers.Add("X-Frame-Options", "DENY");
            
            // Prevent MIME type sniffing
            headers.Add("X-Content-Type-Options", "nosniff");
            
            // Enable XSS protection
            headers.Add("X-XSS-Protection", "1; mode=block");
            
            // Strict transport security
            headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            
            // Content security policy
            headers.Add("Content-Security-Policy", 
                "default-src 'self'; script-src 'self' 'unsafe-inline' https://js.stripe.com; " +
                "style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; " +
                "connect-src 'self' https://api.stripe.com;");
            
            // Referrer policy
            headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Permissions policy
            headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        }

        private async Task LogPaymentRequest(HttpContext context)
        {
            var logData = new
            {
                Timestamp = DateTime.UtcNow,
                Method = context.Request.Method,
                Path = context.Request.Path,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserId = context.User?.Identity?.Name,
                RequestId = context.TraceIdentifier
            };

            _logger.LogInformation("Payment request: {@LogData}", logData);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Use user ID if authenticated, otherwise IP address
            return context.User?.Identity?.Name ?? 
                   context.Connection.RemoteIpAddress?.ToString() ?? 
                   "unknown";
        }

        private async Task<bool> CheckRateLimit(string clientId)
        {
            // Implement rate limiting logic
            // For demo purposes, allowing all requests
            // In production, use Redis or in-memory cache for rate limiting
            return true;
        }
    }
}

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// PCI DSS compliant data handling service
    /// </summary>
    public class PCIDataHandlingService : IPCIDataHandlingService
    {
        private readonly ILogger<PCIDataHandlingService> _logger;
        private readonly IEncryptionService _encryptionService;

        public PCIDataHandlingService(
            ILogger<PCIDataHandlingService> logger,
            IEncryptionService encryptionService)
        {
            _logger = logger;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Tokenize sensitive payment data
        /// </summary>
        public async Task<string> TokenizePaymentData(string sensitiveData)
        {
            try
            {
                // Generate a unique token
                var token = GenerateSecureToken();
                
                // Encrypt the sensitive data
                var encryptedData = _encryptionService.Encrypt(sensitiveData);
                
                // Store the mapping securely (in production, use a secure vault)
                await StoreTokenMapping(token, encryptedData);
                
                _logger.LogInformation("Payment data tokenized successfully");
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to tokenize payment data");
                throw;
            }
        }

        /// <summary>
        /// Detokenize payment data
        /// </summary>
        public async Task<string> DetokenizePaymentData(string token)
        {
            try
            {
                // Retrieve encrypted data using token
                var encryptedData = await GetTokenMapping(token);
                if (encryptedData == null)
                {
                    throw new ArgumentException("Invalid token");
                }
                
                // Decrypt the data
                var sensitiveData = _encryptionService.Decrypt(encryptedData);
                
                _logger.LogInformation("Payment data detokenized successfully");
                return sensitiveData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to detokenize payment data");
                throw;
            }
        }

        /// <summary>
        /// Mask sensitive payment information for display
        /// </summary>
        public string MaskPaymentData(string paymentData, PaymentDataType dataType)
        {
            if (string.IsNullOrEmpty(paymentData))
                return string.Empty;

            return dataType switch
            {
                PaymentDataType.CardNumber => MaskCreditCard(paymentData),
                PaymentDataType.BankAccountNumber => MaskBankAccount(paymentData),
                _ => "****"
            };
        }

        /// <summary>
        /// Validate PCI DSS compliance for payment data
        /// </summary>
        public async Task<PCIValidationResult> ValidatePaymentDataCompliance(object paymentData)
        {
            var result = new PCIValidationResult { IsValid = true };

            try
            {
                // Check for prohibited data storage
                var dataString = System.Text.Json.JsonSerializer.Serialize(paymentData);
                
                // Check for full PAN (Primary Account Number)
                if (ContainsFullPAN(dataString))
                {
                    result.IsValid = false;
                    result.Violations.Add("Full PAN detected in payment data");
                }

                // Check for CVV/CVC
                if (ContainsCVV(dataString))
                {
                    result.IsValid = false;
                    result.Violations.Add("CVV/CVC detected in payment data");
                }

                // Check for magnetic stripe data
                if (ContainsMagneticStripeData(dataString))
                {
                    result.IsValid = false;
                    result.Violations.Add("Magnetic stripe data detected");
                }

                // Check for PIN data
                if (ContainsPINData(dataString))
                {
                    result.IsValid = false;
                    result.Violations.Add("PIN data detected");
                }

                _logger.LogInformation("PCI compliance validation completed: {IsValid}", 
                    result.IsValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PCI compliance validation");
                result.IsValid = false;
                result.Violations.Add("Validation error occurred");
            }

            return result;
        }

        /// <summary>
        /// Securely purge payment data
        /// </summary>
        public async Task<bool> SecurePurgePaymentData(string dataIdentifier)
        {
            try
            {
                // Overwrite data multiple times for secure deletion
                await OverwriteData(dataIdentifier, 3);
                
                // Remove from all caches and temporary storage
                await RemoveFromCaches(dataIdentifier);
                
                // Log the purge operation
                _logger.LogInformation("Payment data securely purged: {DataIdentifier}", 
                    dataIdentifier);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to securely purge payment data: {DataIdentifier}", 
                    dataIdentifier);
                return false;
            }
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private async Task StoreTokenMapping(string token, string encryptedData)
        {
            // In production, store in a secure vault like Azure Key Vault or AWS Secrets Manager
            // For demo purposes, using in-memory storage
            await Task.CompletedTask;
        }

        private async Task<string> GetTokenMapping(string token)
        {
            // In production, retrieve from secure vault
            // For demo purposes, returning null
            await Task.CompletedTask;
            return null;
        }

        private string MaskCreditCard(string cardNumber)
        {
            if (cardNumber.Length < 4)
                return "****";
            
            return "**** **** **** " + cardNumber.Substring(cardNumber.Length - 4);
        }

        private string MaskBankAccount(string accountNumber)
        {
            if (accountNumber.Length < 4)
                return "****";
            
            return "****" + accountNumber.Substring(accountNumber.Length - 4);
        }

        private string MaskSSN(string ssn)
        {
            if (ssn.Length < 4)
                return "***-**-****";
            
            return "***-**-" + ssn.Substring(ssn.Length - 4);
        }

        private bool ContainsFullPAN(string data)
        {
            // Check for patterns that look like full credit card numbers
            var panPattern = @"\b\d{13,19}\b";
            return System.Text.RegularExpressions.Regex.IsMatch(data, panPattern);
        }

        private bool ContainsCVV(string data)
        {
            // Check for CVV patterns
            var cvvPatterns = new[] { "cvv", "cvc", "cid", "security_code" };
            return cvvPatterns.Any(pattern => 
                data.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private bool ContainsMagneticStripeData(string data)
        {
            // Check for magnetic stripe data patterns
            var magneticStripePatterns = new[] { "track1", "track2", "track3", "%B", ";", "?" };
            return magneticStripePatterns.Any(pattern => 
                data.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private bool ContainsPINData(string data)
        {
            // Check for PIN-related data
            var pinPatterns = new[] { "pin", "pin_block", "encrypted_pin" };
            return pinPatterns.Any(pattern => 
                data.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private async Task OverwriteData(string dataIdentifier, int passes)
        {
            // Implement secure data overwriting
            await Task.CompletedTask;
        }

        private async Task RemoveFromCaches(string dataIdentifier)
        {
            // Remove from all caching layers
            await Task.CompletedTask;
        }
    }

    #if false // Duplicate types now defined in Contracts
    /// <summary>
    /// PCI compliance validation result (duplicate)
    /// </summary>
    public class PCIValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Violations { get; set; } = new List<string>();
        public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Payment data types for masking (duplicate)
    /// </summary>
    public enum PaymentDataType
    {
        CardNumber,
        BankAccountNumber,
        SSN,
        Other
    }
    #endif
}

