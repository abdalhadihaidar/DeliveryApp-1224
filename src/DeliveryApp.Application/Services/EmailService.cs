using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// SendPulse email service implementation
    /// </summary>
    public class EmailService : ApplicationService, IEmailService, ITransientDependency
    {
        private readonly SendPulseSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly ILogger<EmailService> _logger;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;

        public EmailService(
            IOptions<SendPulseSettings> settings,
            HttpClient httpClient,
            IDistributedCache cache,
            ILogger<EmailService> logger,
            IStringLocalizer<DeliveryAppResource> localizer)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<EmailSendResultDto> SendVerificationEmailAsync(SendVerificationEmailDto request)
        {
            try
            {
                // Generate verification code
                var verificationCode = GenerateVerificationCode();
                
                // Store verification code in cache
                var cacheKey = $"email_verification:{request.Email}";
                var cacheValue = JsonSerializer.Serialize(new
                {
                    Code = verificationCode,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false
                });

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.VerificationCodeExpiryMinutes)
                };

                await _cache.SetStringAsync(cacheKey, cacheValue, cacheOptions);

                // Send email via SendPulse
                var emailData = new
                {
                    email = new
                    {
                        html = GetVerificationEmailTemplate(request.UserName, verificationCode, request.Language),
                        text = $"Your verification code is: {verificationCode}",
                        subject = request.Language == "ar" ? "رمز التحقق من البريد الإلكتروني" : "Email Verification Code",
                        from = new { name = _settings.FromName, email = _settings.FromEmail },
                        to = new[] { new { name = request.UserName, email = request.Email } }
                    }
                };

                var result = await SendEmailViaSendPulse(emailData);
                
                _logger.LogInformation($"Verification email sent to {request.Email}, Code: {verificationCode}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send verification email to {request.Email}");
                return new EmailSendResultDto
                {
                    Success = false,
                    Message = "Failed to send verification email",
                    ErrorCode = "EMAIL_SEND_FAILED"
                };
            }
        }

        public async Task<EmailSendResultDto> SendPasswordResetEmailAsync(SendPasswordResetEmailDto request)
        {
            try
            {
                // Generate reset code
                var resetCode = GenerateVerificationCode();
                
                // Store reset code in cache
                var cacheKey = $"password_reset:{request.Email}";
                var cacheValue = JsonSerializer.Serialize(new
                {
                    Code = resetCode,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false
                });

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.VerificationCodeExpiryMinutes)
                };

                await _cache.SetStringAsync(cacheKey, cacheValue, cacheOptions);

                // Send email via SendPulse
                var emailData = new
                {
                    email = new
                    {
                        html = GetPasswordResetEmailTemplate(request.UserName, resetCode, request.Language),
                        text = $"Your password reset code is: {resetCode}",
                        subject = request.Language == "ar" ? "رمز إعادة تعيين كلمة المرور" : "Password Reset Code",
                        from = new { name = _settings.FromName, email = _settings.FromEmail },
                        to = new[] { new { name = request.UserName, email = request.Email } }
                    }
                };

                var result = await SendEmailViaSendPulse(emailData);
                
                _logger.LogInformation($"Password reset email sent to {request.Email}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {request.Email}");
                return new EmailSendResultDto
                {
                    Success = false,
                    Message = "Failed to send password reset email",
                    ErrorCode = "EMAIL_SEND_FAILED"
                };
            }
        }

        public async Task<EmailSendResultDto> SendOrderNotificationEmailAsync(SendOrderNotificationEmailDto request)
        {
            try
            {
                // Send email via SendPulse
                var emailData = new
                {
                    email = new
                    {
                        html = GetOrderNotificationEmailTemplate(request.UserName, request.OrderId, request.OrderStatus, request.Language),
                        text = $"Order {request.OrderId} status: {request.OrderStatus}",
                        subject = request.Language == "ar" ? $"تحديث الطلب #{request.OrderId}" : $"Order Update #{request.OrderId}",
                        from = new { name = _settings.FromName, email = _settings.FromEmail },
                        to = new[] { new { name = request.UserName, email = request.Email } }
                    }
                };

                var result = await SendEmailViaSendPulse(emailData);
                
                _logger.LogInformation($"Order notification email sent to {request.Email} for order {request.OrderId}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order notification email to {request.Email}");
                return new EmailSendResultDto
                {
                    Success = false,
                    Message = "Failed to send order notification email",
                    ErrorCode = "EMAIL_SEND_FAILED"
                };
            }
        }

        public async Task<EmailVerificationResultDto> VerifyEmailCodeAsync(VerifyEmailCodeDto request)
        {
            try
            {
                var cacheKey = $"email_verification:{request.Email}";
                var cachedValue = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedValue))
                {
                    return new EmailVerificationResultDto
                    {
                        Success = false,
                        Message = "Verification code not found or expired",
                        IsExpired = true
                    };
                }

                var verificationData = JsonSerializer.Deserialize<dynamic>(cachedValue);
                var storedCode = verificationData.GetProperty("Code").GetString();
                var isUsed = verificationData.GetProperty("IsUsed").GetBoolean();
                var createdAt = verificationData.GetProperty("CreatedAt").GetDateTime();

                if (isUsed)
                {
                    return new EmailVerificationResultDto
                    {
                        Success = false,
                        Message = "Verification code has already been used",
                        IsAlreadyUsed = true
                    };
                }

                if (DateTime.UtcNow.Subtract(createdAt).TotalMinutes > _settings.VerificationCodeExpiryMinutes)
                {
                    await _cache.RemoveAsync(cacheKey);
                    return new EmailVerificationResultDto
                    {
                        Success = false,
                        Message = "Verification code has expired",
                        IsExpired = true
                    };
                }

                if (storedCode != request.VerificationCode)
                {
                    return new EmailVerificationResultDto
                    {
                        Success = false,
                        Message = "Invalid verification code"
                    };
                }

                // Mark as used
                var updatedData = JsonSerializer.Serialize(new
                {
                    Code = storedCode,
                    Email = request.Email,
                    CreatedAt = createdAt,
                    IsUsed = true,
                    UsedAt = DateTime.UtcNow
                });

                await _cache.SetStringAsync(cacheKey, updatedData, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Keep for audit
                });

                _logger.LogInformation($"Email verification successful for {request.Email}");

                return new EmailVerificationResultDto
                {
                    Success = true,
                    Message = "Email verification successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify email code for {request.Email}");
                return new EmailVerificationResultDto
                {
                    Success = false,
                    Message = _localizer["Email:VerificationFailed"]
                };
            }
        }

        private async Task<EmailSendResultDto> SendEmailViaSendPulse(object emailData)
        {
            try
            {
                // Get access token
                var accessToken = await GetSendPulseAccessToken();
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    return new EmailSendResultDto
                    {
                        Success = false,
                        Message = "Failed to authenticate with SendPulse",
                        ErrorCode = "AUTH_FAILED"
                    };
                }

                // Send email
                var json = JsonSerializer.Serialize(emailData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync("https://api.sendpulse.com/smtp/emails", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    return new EmailSendResultDto
                    {
                        Success = true,
                        Message = _localizer["Email:EmailSent"],
                        MessageId = result.TryGetProperty("id", out var id) ? id.GetString() : null
                    };
                }
                else
                {
                    _logger.LogError($"SendPulse API error: {response.StatusCode} - {responseContent}");
                    return new EmailSendResultDto
                    {
                        Success = false,
                        Message = "Failed to send email via SendPulse",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via SendPulse");
                return new EmailSendResultDto
                {
                    Success = false,
                    Message = "Email service error",
                    ErrorCode = "SERVICE_ERROR"
                };
            }
        }

        private async Task<string> GetSendPulseAccessToken()
        {
            try
            {
                var cacheKey = "sendpulse_access_token";
                var cachedToken = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedToken))
                {
                    return cachedToken;
                }

                // Get new token
                var authData = new
                {
                    grant_type = "client_credentials",
                    client_id = _settings.ApiUserId,
                    client_secret = _settings.ApiSecret
                };

                var json = JsonSerializer.Serialize(authData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.sendpulse.com/oauth/access_token", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var accessToken = result.GetProperty("access_token").GetString();
                    var expiresIn = result.GetProperty("expires_in").GetInt32();

                    // Cache token for 90% of its lifetime
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresIn * 0.9)
                    };

                    await _cache.SetStringAsync(cacheKey, accessToken, cacheOptions);
                    return accessToken;
                }

                _logger.LogError($"Failed to get SendPulse access token: {response.StatusCode} - {responseContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SendPulse access token");
                return null;
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string GetVerificationEmailTemplate(string userName, string code, string language)
        {
            if (language == "ar")
            {
                return $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; direction: rtl; text-align: right;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>مرحباً {userName}</h2>
                        <p>رمز التحقق من البريد الإلكتروني الخاص بك هو:</p>
                        <div style='background: #f5f5f5; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; margin: 20px 0;'>
                            {code}
                        </div>
                        <p>هذا الرمز صالح لمدة {_settings.VerificationCodeExpiryMinutes} دقيقة.</p>
                        <p>إذا لم تطلب هذا الرمز، يرجى تجاهل هذا البريد الإلكتروني.</p>
                    </div>
                </body>
                </html>";
            }
            else
            {
                return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Hello {userName}</h2>
                        <p>Your email verification code is:</p>
                        <div style='background: #f5f5f5; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; margin: 20px 0;'>
                            {code}
                        </div>
                        <p>This code is valid for {_settings.VerificationCodeExpiryMinutes} minutes.</p>
                        <p>If you didn't request this code, please ignore this email.</p>
                    </div>
                </body>
                </html>";
            }
        }

        private string GetPasswordResetEmailTemplate(string userName, string code, string language)
        {
            if (language == "ar")
            {
                return $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; direction: rtl; text-align: right;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>مرحباً {userName}</h2>
                        <p>رمز إعادة تعيين كلمة المرور الخاص بك هو:</p>
                        <div style='background: #f5f5f5; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; margin: 20px 0;'>
                            {code}
                        </div>
                        <p>هذا الرمز صالح لمدة {_settings.VerificationCodeExpiryMinutes} دقيقة.</p>
                        <p>إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني.</p>
                    </div>
                </body>
                </html>";
            }
            else
            {
                return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Hello {userName}</h2>
                        <p>Your password reset code is:</p>
                        <div style='background: #f5f5f5; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; margin: 20px 0;'>
                            {code}
                        </div>
                        <p>This code is valid for {_settings.VerificationCodeExpiryMinutes} minutes.</p>
                        <p>If you didn't request a password reset, please ignore this email.</p>
                    </div>
                </body>
                </html>";
            }
        }

        private string GetOrderNotificationEmailTemplate(string userName, string orderId, string status, string language)
        {
            if (language == "ar")
            {
                return $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; direction: rtl; text-align: right;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>مرحباً {userName}</h2>
                        <p>تم تحديث حالة طلبك #{orderId}</p>
                        <div style='background: #e8f5e8; padding: 20px; text-align: center; font-size: 18px; font-weight: bold; margin: 20px 0;'>
                            الحالة الحالية: {status}
                        </div>
                        <p>يمكنك تتبع طلبك من خلال التطبيق.</p>
                    </div>
                </body>
                </html>";
            }
            else
            {
                return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Hello {userName}</h2>
                        <p>Your order #{orderId} has been updated</p>
                        <div style='background: #e8f5e8; padding: 20px; text-align: center; font-size: 18px; font-weight: bold; margin: 20px 0;'>
                            Current Status: {status}
                        </div>
                        <p>You can track your order through the app.</p>
                    </div>
                </body>
                </html>";
            }
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var emailData = new
                {
                    email = new
                    {
                        subject = subject,
                        html = body,
                        text = body,
                        to = new[]
                        {
                            new { email = email }
                        }
                    }
                };

                var result = await SendEmailViaSendPulse(emailData);
                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                return false;
            }
        }
    }
}

