using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Services
{
    public class SmsService : ApplicationService, ISmsService, ITransientDependency
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache<string> _cache;
        private readonly ILogger<SmsService> _logger;

        public SmsService(
            IConfiguration configuration,
            IDistributedCache<string> cache,
            ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            
            // Initialize Twilio
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string countryCode)
        {
            try
            {
                // Generate 6-digit verification code
                var code = new Random().Next(100000, 999999).ToString();
                
                // Format phone number
                var formattedNumber = $"{countryCode}{phoneNumber}";
                
                // Store code in cache for 10 minutes
                var cacheKey = $"sms_verification_{formattedNumber}";
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _cache.SetAsync(cacheKey, code, cacheOptions);
                
                // Send SMS via Twilio
                var message = await MessageResource.CreateAsync(
                    body: $"Your verification code is: {code}. Valid for 10 minutes.",
                    from: new PhoneNumber(_configuration["Twilio:FromNumber"]),
                    to: new PhoneNumber(formattedNumber)
                );

                _logger.LogInformation($"SMS sent successfully to {formattedNumber}. SID: {message.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SMS to {phoneNumber}");
                return false;
            }
        }

        public async Task<bool> VerifyCodeAsync(string phoneNumber, string code)
        {
            try
            {
                var cacheKey = $"sms_verification_{phoneNumber}";
                var storedCode = await _cache.GetAsync(cacheKey);
                
                if (storedCode == null)
                {
                    _logger.LogWarning($"Verification code expired or not found for {phoneNumber}");
                    return false;
                }

                if (storedCode == code)
                {
                    // Remove code from cache after successful verification
                    await _cache.RemoveAsync(cacheKey);
                    _logger.LogInformation($"Phone number {phoneNumber} verified successfully");
                    return true;
                }

                _logger.LogWarning($"Invalid verification code for {phoneNumber}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying code for {phoneNumber}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetCodeAsync(string phoneNumber)
        {
            try
            {
                // Generate 6-digit reset code
                var code = new Random().Next(100000, 999999).ToString();
                
                // Store code in cache for 15 minutes
                var cacheKey = $"password_reset_{phoneNumber}";
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                };
                await _cache.SetAsync(cacheKey, code, cacheOptions);
                
                // Send SMS
                var message = await MessageResource.CreateAsync(
                    body: $"Your password reset code is: {code}. Valid for 15 minutes.",
                    from: new PhoneNumber(_configuration["Twilio:FromNumber"]),
                    to: new PhoneNumber(phoneNumber)
                );

                _logger.LogInformation($"Password reset SMS sent to {phoneNumber}. SID: {message.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset SMS to {phoneNumber}");
                return false;
            }
        }

        public async Task<bool> SendOrderNotificationAsync(string phoneNumber, string message)
        {
            try
            {
                var smsMessage = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_configuration["Twilio:FromNumber"]),
                    to: new PhoneNumber(phoneNumber)
                );

                _logger.LogInformation($"Order notification SMS sent to {phoneNumber}. SID: {smsMessage.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order notification SMS to {phoneNumber}");
                return false;
            }
        }
    }
}

