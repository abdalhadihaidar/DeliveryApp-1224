using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using DeliveryApp.Application.Extensions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Security.Claims;
using Volo.Abp.SecurityLog;
using Volo.Abp.Guids;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Application.Services;
using System.Text.Json;
using Volo.Abp.Data;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Enhanced authentication service supporting email and phone authentication
    /// </summary>
    [RemoteService]
    public class AuthService : ApplicationService, IAuthService, ITransientDependency
    {
        private readonly AuthSettings _authSettings;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AuthService> _logger;
        private readonly IdentityUserManager _userManager;
        private readonly IdentitySecurityLogManager _securityLogManager;
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;
        private readonly HttpClient _httpClient;

        public AuthService(
            IOptions<AuthSettings> authSettings,
            IEmailService emailService,
            IDistributedCache cache,
            ILogger<AuthService> logger,
            IdentityUserManager userManager,
            IdentitySecurityLogManager securityLogManager,
            IRepository<Restaurant, Guid> restaurantRepository,
            IUserRepository userRepository,
            IConfiguration configuration,
            IStringLocalizer<DeliveryAppResource> localizer,
            HttpClient httpClient)
        {
            _authSettings = authSettings.Value;
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
            _userManager = userManager;
            _securityLogManager = securityLogManager;
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _localizer = localizer;
            _httpClient = httpClient;
        }

        public async Task<AuthResultDto> RegisterWithEmailAsync(RegisterWithEmailDto request)
        {
            try
            {
                // Role enum validation is handled by the framework

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:UserExists"],
                        ErrorCode = "USER_EXISTS"
                    };
                }

                // Create new user as AppUser to avoid duplicate tracking
                var user = new AppUser(
                    GuidGenerator.Create(),
                    request.Email,
                    request.Email,
                    CurrentTenant.Id
                );

                user.SetEmailConfirmed(false);
                user.Name = request.FirstName;
                user.Surname = request.LastName;
                
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    user.SetPhoneNumber(request.PhoneNumber, false);
                }

                // Set PreferredLanguage property
                user.SetProperty("PreferredLanguage", request.Language);

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:RegistrationFailed"],
                        ErrorCode = "REGISTRATION_FAILED"
                    };
                }

                // Assign user to role based on Role enum (do this before email verification)
                var roleName = MapRoleToRoleName(request.Role);
                await _userManager.AddToRoleAsync(user, roleName);

                // Set additional AppUser-specific properties
                user.ProfileImageUrl = string.Empty;
                user.ReviewStatus = DeliveryApp.Domain.Enums.ReviewStatus.Pending;
                user.ReviewReason = null;

                // If user is a restaurant owner, create a default restaurant
                if (request.Role == UserRole.RestaurantOwner)
                {
                    await CreateDefaultRestaurantForOwnerAsync(user.Id);
                }

                // Send verification email
                if (_authSettings.RequireEmailVerification)
                {
                    await _emailService.SendVerificationEmailAsync(new SendVerificationEmailDto
                    {
                        Email = request.Email,
                        UserName = $"{request.FirstName} {request.LastName}",
                        Language = request.Language
                    });

                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Registration successful. Please verify your email.",
                        RequiresVerification = true,
                        VerificationType = "Email"
                    };
                }

                // Auto-confirm email if verification not required
                user.SetEmailConfirmed(true);
                await _userManager.UpdateAsync(user);

                // Generate tokens for the newly registered user
                var tokenResult = await RequestTokenAsync(request.Email, request.Password);

                _logger.LogInformation($"User registered successfully with email: {request.Email}");

                return new AuthResultDto
                {
                    Success = true,
                    Message = "Registration successful",
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresAt = tokenResult.ExpiresAt,
                    User = await MapToUserInfoDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user with email: {request.Email}");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:RegistrationFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> RegisterWithPhoneAsync(RegisterWithPhoneDto request)
        {
            try
            {
                // Role enum validation is handled by the framework

                // Check if user already exists
                var existingUser = await _userManager.FindByPhoneNumberAsync(request.PhoneNumber);
                if (existingUser != null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:UserExistsPhone"],
                        ErrorCode = "USER_EXISTS"
                    };
                }

                // Create new user as AppUser to avoid duplicate tracking
                var user = new AppUser(
                    GuidGenerator.Create(),
                    request.PhoneNumber,
                    request.Email ?? request.PhoneNumber,
                    CurrentTenant.Id
                );

                user.SetPhoneNumber(request.PhoneNumber, false);
                user.Name = request.FirstName;
                user.Surname = request.LastName;
                
                if (!string.IsNullOrEmpty(request.Email))
                {
                    user.SetEmailConfirmed(false);
                }

                // Set PreferredLanguage property
                user.SetProperty("PreferredLanguage", request.Language);

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:RegistrationFailed"],
                        ErrorCode = "REGISTRATION_FAILED"
                    };
                }

                // Assign user to role based on Role enum (do this before email verification)
                var roleName = MapRoleToRoleName(request.Role);
                await _userManager.AddToRoleAsync(user, roleName);

                // Set additional properties specific to AppUser
                user.ProfileImageUrl = string.Empty;
                user.ReviewStatus = DeliveryApp.Domain.Enums.ReviewStatus.Pending;
                user.ReviewReason = null;

                // If user is a restaurant owner, create a default restaurant
                if (request.Role == UserRole.RestaurantOwner)
                {
                    await CreateDefaultRestaurantForOwnerAsync(user.Id);
                }

                // For phone registration, we'll use email verification if email is provided
                // Otherwise, consider phone verified for now (SMS integration can be added later)
                if (!string.IsNullOrEmpty(request.Email) && _authSettings.RequireEmailVerification)
                {
                    await _emailService.SendVerificationEmailAsync(new SendVerificationEmailDto
                    {
                        Email = request.Email,
                        UserName = $"{request.FirstName} {request.LastName}",
                        Language = request.Language
                    });

                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Registration successful. Please verify your email.",
                        RequiresVerification = true,
                        VerificationType = "Email"
                    };
                }

                // Auto-confirm if no verification required
                user.SetPhoneNumberConfirmed(true);
                if (!string.IsNullOrEmpty(request.Email))
                {
                    user.SetEmailConfirmed(true);
                }
                await _userManager.UpdateAsync(user);

                // Generate tokens for the newly registered user
                var tokenResult = await RequestTokenAsync(request.PhoneNumber, request.Password);

                _logger.LogInformation($"User registered successfully with phone: {request.PhoneNumber}");

                return new AuthResultDto
                {
                    Success = true,
                    Message = "Registration successful",
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresAt = tokenResult.ExpiresAt,
                    User = await MapToUserInfoDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user with phone: {request.PhoneNumber}");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:RegistrationFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> LoginWithEmailAsync(LoginWithEmailDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:InvalidCredentials"],
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
                    {
                        Identity = "Identity",
                        Action = "LoginFailed",
                        UserName = user.UserName,
                        ExtraProperties = { { "Reason", "InvalidPassword" } }
                    });

                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:InvalidCredentials"],
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                // Check if email verification is required
              /*  if (_authSettings.RequireEmailVerification && !user.EmailConfirmed)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Please verify your email before logging in",
                        ErrorCode = "EMAIL_NOT_VERIFIED",
                        RequiresVerification = true,
                        VerificationType = "Email"
                    };
                }*/

                // Check if user is active
                if (!user.IsActive)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Your account has been deactivated",
                        ErrorCode = "ACCOUNT_DEACTIVATED"
                    };
                }

                // Generate tokens using OpenIddict
                var tokenResult = await RequestTokenAsync(request.Email, request.Password);

                await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
                {
                    Identity = "Identity",
                    Action = "LoginSucceeded",
                    UserName = user.UserName
                });

                _logger.LogInformation($"User logged in successfully with email: {request.Email}");

                try
                {
                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Login successful",
                        AccessToken = tokenResult.AccessToken,
                        RefreshToken = tokenResult.RefreshToken,
                        ExpiresAt = tokenResult.ExpiresAt,
                        User = await MapToUserInfoDtoAsync(user)
                    };
                }
                catch (InvalidOperationException ex)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = ex.Message,
                        ErrorCode = "INVALID_USER_ROLE"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging in user with email: {request.Email}");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:LoginFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> LoginWithPhoneAsync(LoginWithPhoneDto request)
        {
            try
            {
                var user = await _userManager.FindByPhoneNumberAsync(request.PhoneNumber);
                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid phone number or password",
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
                    {
                        Identity = "Identity",
                        Action = "LoginFailed",
                        UserName = user.UserName,
                        ExtraProperties = { { "Reason", "InvalidPassword" } }
                    });

                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid phone number or password",
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                // Check if phone verification is required
                if (_authSettings.RequirePhoneVerification && !user.PhoneNumberConfirmed)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Please verify your phone number before logging in",
                        ErrorCode = "PHONE_NOT_VERIFIED",
                        RequiresVerification = true,
                        VerificationType = "Phone"
                    };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Your account has been deactivated",
                        ErrorCode = "ACCOUNT_DEACTIVATED"
                    };
                }

                // Generate tokens using OpenIddict
                var tokenResult = await RequestTokenAsync(request.PhoneNumber, request.Password);

                await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
                {
                    Identity = "Identity",
                    Action = "LoginSucceeded",
                    UserName = user.UserName
                });

                _logger.LogInformation($"User logged in successfully with phone: {request.PhoneNumber}");

                try
                {
                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Login successful",
                        AccessToken = tokenResult.AccessToken,
                        RefreshToken = tokenResult.RefreshToken,
                        ExpiresAt = tokenResult.ExpiresAt,
                        User = await MapToUserInfoDtoAsync(user)
                    };
                }
                catch (InvalidOperationException ex)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = ex.Message,
                        ErrorCode = "INVALID_USER_ROLE"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging in user with phone: {request.PhoneNumber}");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:LoginFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> VerifyEmailAsync(VerifyEmailDto request)
        {
            try
            {
                var verificationResult = await _emailService.VerifyEmailCodeAsync(new VerifyEmailCodeDto
                {
                    Email = request.Email,
                    VerificationCode = request.VerificationCode
                });

                if (!verificationResult.Success)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = verificationResult.Message,
                        ErrorCode = "VERIFICATION_FAILED"
                    };
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                // Confirm email
                user.SetEmailConfirmed(true);
                await _userManager.UpdateAsync(user);

                // NOTE: Manual JWT generation removed. Use OpenIddict's /connect/token endpoint for authentication.

                _logger.LogInformation($"Email verified successfully for user: {request.Email}");

                                    return new AuthResultDto
                    {
                        Success = true,
                        Message = _localizer["Auth:EmailVerified"],
                        AccessToken = "N/A", // No token generated here
                        RefreshToken = "N/A", // No token generated here
                        ExpiresAt = DateTime.MinValue, // No token generated here
                        User = await MapToUserInfoDtoAsync(user)
                    };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying email: {request.Email}");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:EmailVerificationFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> VerifyPhoneAsync(VerifyPhoneDto request)
        {
            // For now, we'll implement a simple verification
            // In a real implementation, you would integrate with SMS service
            try
            {
                var user = await _userManager.FindByPhoneNumberAsync(request.PhoneNumber);
                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                // For demo purposes, accept any 6-digit code
                if (request.VerificationCode.Length == 6 && request.VerificationCode.All(char.IsDigit))
                {
                    user.SetPhoneNumberConfirmed(true);
                    await _userManager.UpdateAsync(user);

                    // NOTE: Manual JWT generation removed. Use OpenIddict's /connect/token endpoint for authentication.

                    _logger.LogInformation($"Phone verified successfully for user: {request.PhoneNumber}");

                    return new AuthResultDto
                    {
                        Success = true,
                        Message = _localizer["Auth:PhoneVerified"],
                        AccessToken = "N/A", // No token generated here
                        RefreshToken = "N/A", // No token generated here
                        ExpiresAt = DateTime.MinValue, // No token generated here
                        User = await MapToUserInfoDtoAsync(user)
                    };
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid verification code",
                    ErrorCode = "INVALID_CODE"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying phone: {request.PhoneNumber}");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:PhoneVerificationFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> ResendVerificationCodeAsync(ResendVerificationCodeDto request)
        {
            try
            {
                if (request.VerificationType == "Email" && !string.IsNullOrEmpty(request.Email))
                {
                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user == null)
                    {
                        return new AuthResultDto
                        {
                            Success = false,
                            Message = "User not found",
                            ErrorCode = "USER_NOT_FOUND"
                        };
                    }

                    await _emailService.SendVerificationEmailAsync(new SendVerificationEmailDto
                    {
                        Email = request.Email,
                        UserName = $"{user.Name} {user.Surname}",
                        Language = request.Language
                    });

                    return new AuthResultDto
                    {
                        Success = true,
                        Message = _localizer["Auth:VerificationEmailSent"]
                    };
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid verification type or missing information",
                    ErrorCode = "INVALID_REQUEST"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code");
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Failed to resend verification code",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto request)
        {
            try
            {
                IdentityUser user = null;

                if (!string.IsNullOrEmpty(request.Email))
                {
                    user = await _userManager.FindByEmailAsync(request.Email);
                }
                else if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    user = await _userManager.FindByPhoneNumberAsync(request.PhoneNumber);
                }

                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                // For demo purposes, accept any 6-digit reset code
                if (request.ResetCode.Length == 6 && request.ResetCode.All(char.IsDigit))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Password reset successfully for user: {user.UserName}");

                        return new AuthResultDto
                        {
                            Success = true,
                            Message = _localizer["Auth:PasswordReset"]
                        };
                    }
                    else
                    {
                        return new AuthResultDto
                        {
                            Success = false,
                            Message = _localizer["Auth:RegistrationFailed"],
                            ErrorCode = "PASSWORD_RESET_FAILED"
                        };
                    }
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid reset code",
                    ErrorCode = "INVALID_CODE"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:PasswordResetFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> ChangePasswordAsync(ChangePasswordDto request)
        {
            try
            {
                var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Password changed successfully for user: {user.UserName}");

                    return new AuthResultDto
                    {
                        Success = true,
                        Message = _localizer["Auth:PasswordChanged"]
                    };
                }
                else
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:RegistrationFailed"],
                        ErrorCode = "PASSWORD_CHANGE_FAILED"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return new AuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:PasswordChangeFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto request)
        {
            try
            {
                // Validate refresh token
                var cacheKey = $"refresh_token:{request.RefreshToken}";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedData))
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token",
                        ErrorCode = "INVALID_REFRESH_TOKEN"
                    };
                }

                var tokenData = JsonSerializer.Deserialize<RefreshTokenData>(cachedData);
                var user = await _userManager.GetByIdAsync(Guid.Parse(tokenData.UserId));

                if (user == null || !user.IsActive)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "User not found or inactive",
                        ErrorCode = "USER_INACTIVE"
                    };
                }

                // Generate new tokens using OpenIddict
                var tokenResult = await RequestTokenAsync(user.Email, ""); // Password not needed for refresh

                // Remove old refresh token
                await _cache.RemoveAsync(cacheKey);

                _logger.LogInformation($"Token refreshed successfully for user: {user.UserName}");

                return new AuthResultDto
                {
                    Success = true,
                    Message = _localizer["Auth:TokenRefreshed"],
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresAt = tokenResult.ExpiresAt,
                    User = await MapToUserInfoDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new AuthResultDto
                {
                    Success = false,
                    Message = _localizer["Auth:TokenRefreshFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<AuthResultDto> LogoutAsync(LogoutDto request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.RefreshToken))
                {
                    var cacheKey = $"refresh_token:{request.RefreshToken}";
                    await _cache.RemoveAsync(cacheKey);
                }

                _logger.LogInformation(_localizer["Auth:LogoutSuccess"]);

                return new AuthResultDto
                {
                    Success = true,
                    Message = _localizer["Auth:LogoutSuccess"]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new AuthResultDto
                {
                    Success = false,
                    Message = _localizer["Auth:LogoutFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        private string MapRoleToRoleName(UserRole role)
        {
            return role switch
            {
                UserRole.Customer => "customer",
                UserRole.Delivery => "delivery",
                UserRole.RestaurantOwner => "restaurant_owner",
                _ => throw new ArgumentException($"Invalid role: {role}")
            };
        }

        private async Task<UserInfoDto> MapToUserInfoDtoAsync(IdentityUser user)
        {
            // Get user roles to determine user type
            var roles = await _userManager.GetRolesAsync(user);
            UserRole role;
            
            if (roles.Contains("restaurant_owner"))
            {
                role = UserRole.RestaurantOwner;
            }
            else if (roles.Contains("delivery"))
            {
                role = UserRole.Delivery;
            }
            else if (roles.Contains("customer"))
            {
                role = UserRole.Customer;
            }
            else
            {
                throw new InvalidOperationException($"User {user.Email} has no valid role assigned. Expected one of: restaurant_owner, delivery, customer. Found roles: {string.Join(", ", roles)}");
            }
            
            return new UserInfoDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.Name,
                LastName = user.Surname,
                Role = role,
                IsEmailVerified = user.EmailConfirmed,
                IsPhoneVerified = user.PhoneNumberConfirmed,
                IsActive = user.IsActive,
                CreatedAt = user.CreationTime,
                PreferredLanguage = user.GetProperty<string>("PreferredLanguage") ?? "en"
            };
        }

        private UserInfoDto MapToUserInfoDto(IdentityUser user)
        {
            throw new InvalidOperationException("This method is deprecated. Use MapToUserInfoDtoAsync instead to get proper role mapping.");
        }

        private class TokenResult
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        private class RefreshTokenData
        {
            public string UserId { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private async Task CreateDefaultRestaurantForOwnerAsync(Guid userId)
        {
            try
            {
                var existingRestaurant = await _restaurantRepository.FirstOrDefaultAsync(r => r.OwnerId == userId);
                if (existingRestaurant == null)
                {
                    var restaurantId = GuidGenerator.Create();
                    var restaurant = new Restaurant(restaurantId)
                    {
                        OwnerId = userId,
                        Name = "مطعم جديد", // New Restaurant in Arabic
                        Description = "مطعم جديد ينتظر الموافقة من الإدارة", // New restaurant waiting for admin approval
                        CategoryId = null, // Will be set later by admin
                        ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1074&q=80",
                        Rating = 0, // New restaurant starts with no rating
                        DeliveryTime = "30-45 دقيقة", // 30-45 minutes
                        DeliveryFee = 5.00m,
                        MinimumOrderAmount = 10.00m,
                        Tags = new List<string> { "مطعم جديد", "في انتظار الموافقة" }, // New restaurant, pending approval
                        Address = new Address(GuidGenerator.Create())
                        {
                            Street = "شارع الرئيسي",
                            City = "دمشق",
                            State = "دمشق",
                            ZipCode = "10001",
                            FullAddress = "شارع الرئيسي، دمشق",
                            Latitude = 33.5138,
                            Longitude = 36.2765
                        }
                    };

                    await _restaurantRepository.InsertAsync(restaurant);
                    _logger.LogInformation($"Created default restaurant '{restaurant.Name}' for user {userId}");
                }
                else
                {
                    _logger.LogInformation($"Default restaurant already exists for user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating default restaurant for user {userId}");
                // Don't throw - we don't want to fail registration if restaurant creation fails
            }
        }

        private async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> RequestTokenAsync(string username, string password)
        {
            try
            {
                // Exchange username/password against the OpenIddict token endpoint to get tokens
                // Use injected HttpClient instead of creating new instances to prevent memory leaks
                var tokenEndpoint = new Uri(new Uri(_configuration["App:SelfUrl"] ?? "https://localhost:44356"), "/connect/token");

                var form = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password },
                    { "client_id", "DeliveryApp_App" },
                    { "client_secret", "YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP" },
                    { "scope", "DeliveryApp offline_access" }
                };

                var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenIddict token request failed: {Status} {Content}", response.StatusCode, content);
                    throw new Exception(_localizer["Auth:TokenRequestFailed"]);
                }

                using var jsonDoc = JsonDocument.Parse(content);
                var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
                var refreshToken = jsonDoc.RootElement.TryGetProperty("refresh_token", out var rtEl) ? rtEl.GetString() : null;
                var expiresIn = jsonDoc.RootElement.TryGetProperty("expires_in", out var eiEl) ? eiEl.GetInt32() : 86400;

                var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

                return (accessToken!, refreshToken, expiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token");
                throw;
            }
        }
    }
}

