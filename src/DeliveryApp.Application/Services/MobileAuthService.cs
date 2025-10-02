using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Web.Mvc;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;

namespace DeliveryApp.Application.Services
{
    [RemoteService(IsEnabled = true)]
    public class MobileAuthService : ApplicationService, IMobileAuthService, ITransientDependency
    {
        private readonly IdentityUserManager _userManager;
        private readonly ILogger<MobileAuthService> _logger;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        
        // Thread-safe in-memory storage for refresh tokens with automatic cleanup
        private static readonly ConcurrentDictionary<string, RefreshTokenInfo> _refreshTokens = new();
        private static readonly Timer _cleanupTimer;
        private static readonly object _cleanupLock = new object();

        static MobileAuthService()
        {
            // Initialize cleanup timer to run every 5 minutes
            _cleanupTimer = new Timer(CleanupExpiredTokensCallback, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public MobileAuthService(
            IdentityUserManager userManager,
            ILogger<MobileAuthService> logger,
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictScopeManager scopeManager,
            IConfiguration configuration,
            IUserRepository userRepository,
            IStringLocalizer<DeliveryAppResource> localizer,
            HttpClient httpClient,
            IDistributedCache cache)
        {
            _userManager = userManager;
            _logger = logger;
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
            _configuration = configuration;
            _userRepository = userRepository;
            _localizer = localizer;
            _httpClient = httpClient;
            _cache = cache;
        }

        [HttpPost]
        [Route("api/app/mobile-auth/register-with-email")]
        [System.Web.Mvc.AllowAnonymous]
        public async Task<MobileAuthResultDto> RegisterWithEmailAsync(RegisterWithEmailDto request)
        {
            try
            {
                _logger.LogInformation($"Mobile registration attempt for email: {request.Email}, Role: {request.Role}");

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        ErrorCode = "USER_EXISTS"
                    };
                }

                // Create new user (AppUser inherits from IdentityUser, so this is already an AppUser)
                var user = new AppUser(
                    GuidGenerator.Create(),
                    request.Email,
                    request.Email,
                    CurrentTenant.Id
                );

                user.Name = request.FirstName;
                user.Surname = request.LastName;
                user.SetProperty("PreferredLanguage", request.Language);
                
                // Set AppUser-specific properties
                user.ProfileImageUrl = string.Empty;
                user.ReviewStatus = DeliveryApp.Domain.Enums.ReviewStatus.Pending;
                user.ReviewReason = null;

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                        ErrorCode = "REGISTRATION_FAILED"
                    };
                }

                // Assign role
                var roleName = MapRoleToRoleName(request.Role);
                await _userManager.AddToRoleAsync(user, roleName);

                // Auto-confirm email
                user.SetEmailConfirmed(true);
                await _userManager.UpdateAsync(user);

                // Generate tokens for the newly registered user
                var tokenResult = await RequestTokenAsync(request.Email, request.Password);

                _logger.LogInformation($"Mobile user registered successfully with email: {request.Email}");

                return new MobileAuthResultDto
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
                _logger.LogError(ex, $"Error registering mobile user with email: {request.Email}");
                return new MobileAuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:RegistrationFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }


        [HttpPost]
        [Route("api/app/mobile-auth/login-with-email")]
        [System.Web.Mvc.AllowAnonymous]
        public async Task<MobileAuthResultDto> LoginWithEmailAsync(LoginWithEmailDto request)
        {
            try
            {
                // Check cache first for user lookup
                var cacheKey = $"user_email_{request.Email}";
                var cachedUser = await _cache.GetStringAsync(cacheKey);
                IdentityUser user = null;
                
                if (!string.IsNullOrEmpty(cachedUser))
                {
                    // Try to find user by ID from cache
                    var userId = cachedUser;
                    user = await _userManager.FindByIdAsync(userId);
                }
                
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(request.Email);
                    if (user != null)
                    {
                        // Cache user ID for future lookups (5 minutes)
                        await _cache.SetStringAsync(cacheKey, user.Id.ToString(), new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        });
                    }
                }
                
                if (user == null)
                {
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:InvalidCredentials"],
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = _localizer["Auth:InvalidCredentials"],
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                var appUser = user as DeliveryApp.Domain.Entities.AppUser;
                if (appUser != null && appUser.ReviewStatus != DeliveryApp.Domain.Enums.ReviewStatus.Accepted)
                {
                    string message = appUser.ReviewStatus switch
                    {
                        DeliveryApp.Domain.Enums.ReviewStatus.Pending => _localizer["Auth:AccountPending"],
                        DeliveryApp.Domain.Enums.ReviewStatus.Rejected => _localizer["Auth:AccountRejected", appUser.ReviewReason ?? _localizer["Auth:ContactSupport"]],
                        _ => _localizer["Auth:AccountNotApproved"]
                    };
                    
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = message,
                        ErrorCode = appUser.ReviewStatus.ToString().ToUpper()
                    };
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault() ?? "customer";

                // Issue token via OpenIddict server
                var tokenResult = await RequestTokenAsync(request.Email, request.Password);

                _logger.LogInformation($"Mobile user logged in successfully with email: {request.Email}");

                return new MobileAuthResultDto
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresAt = tokenResult.ExpiresAt,
                    User = await MapToUserInfoDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging in mobile user with email: {request.Email}");
                return new MobileAuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:LoginFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        [HttpPost]
        [Route("api/app/mobile-auth/refresh-token")]
        [System.Web.Mvc.AllowAnonymous]
        public async Task<MobileAuthResultDto> RefreshTokenAsync(RefreshTokenDto request)
        {
            try
            {
                // Clean up expired refresh tokens
                CleanupExpiredRefreshTokens();
                
                // Check if refresh token exists and is valid
                if (!_refreshTokens.TryGetValue(request.RefreshToken, out var tokenInfo))
                {
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token",
                        ErrorCode = "INVALID_REFRESH_TOKEN"
                    };
                }

                // Check if refresh token has expired
                if (tokenInfo.ExpiresAt < DateTime.UtcNow)
                {
                    _refreshTokens.TryRemove(request.RefreshToken, out _);
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = "Refresh token has expired",
                        ErrorCode = "REFRESH_TOKEN_EXPIRED"
                    };
                }

                // Get the user
                var user = await _userManager.FindByIdAsync(tokenInfo.UserId.ToString());
                if (user == null || !user.IsActive)
                {
                    _refreshTokens.TryRemove(request.RefreshToken, out _);
                    return new MobileAuthResultDto
                    {
                        Success = false,
                        Message = "User not found or inactive",
                        ErrorCode = "USER_INACTIVE"
                    };
                }

                // Generate new tokens
                var newTokenResult = await RequestTokenAsync(user.Email, null); // We don't need password for refresh
                
                // Remove old refresh token
                _refreshTokens.TryRemove(request.RefreshToken, out _);

                _logger.LogInformation($"Token refreshed successfully for user: {user.Email}");

                return new MobileAuthResultDto
                {
                    Success = true,
                    Message = _localizer["Auth:TokenRefreshed"],
                    AccessToken = newTokenResult.AccessToken,
                    RefreshToken = newTokenResult.RefreshToken,
                    ExpiresAt = newTokenResult.ExpiresAt,
                    User = await MapToUserInfoDtoAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new MobileAuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:TokenRefreshFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        [HttpPost]
        [Route("api/app/mobile-auth/logout")]
        [System.Web.Mvc.AllowAnonymous]
        public async Task<MobileAuthResultDto> LogoutAsync(LogoutDto request)
        {
            try
            {
                // Clean up expired refresh tokens
                CleanupExpiredRefreshTokens();
                
                // If refresh token is provided, invalidate it
                if (!string.IsNullOrEmpty(request.RefreshToken))
                {
                    _refreshTokens.TryRemove(request.RefreshToken, out _);
                }

                return new MobileAuthResultDto
                {
                    Success = true,
                    Message = "Logout successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new MobileAuthResultDto
                {
                    Success = false,
                        Message = _localizer["Auth:LogoutFailed"],
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        private class RefreshTokenInfo
        {
            public Guid UserId { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        private static void CleanupExpiredTokensCallback(object state)
        {
            lock (_cleanupLock)
            {
                try
                {
                    var expiredTokens = _refreshTokens.Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow).ToList();
                    foreach (var expiredToken in expiredTokens)
                    {
                        _refreshTokens.TryRemove(expiredToken.Key, out _);
                    }
                    
                    if (expiredTokens.Any())
                    {
                        // Use a static logger or console output since we can't access instance logger
                        Console.WriteLine($"Cleaned up {expiredTokens.Count} expired refresh tokens");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during token cleanup: {ex.Message}");
                }
            }
        }

        private void CleanupExpiredRefreshTokens()
        {
            CleanupExpiredTokensCallback(null);
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
                    throw new Exception("Token request failed");
                }

                using var jsonDoc = JsonDocument.Parse(content);
                var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
                var refreshToken = jsonDoc.RootElement.TryGetProperty("refresh_token", out var rtEl) ? rtEl.GetString() : null;
                var expiresIn = jsonDoc.RootElement.TryGetProperty("expires_in", out var eiEl) ? eiEl.GetInt32() : 86400;

                var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    // Optional: keep compatibility with existing in-memory refresh cache
                    _refreshTokens[refreshToken] = new RefreshTokenInfo
                    {
                        UserId = (await _userManager.FindByEmailAsync(username)).Id,
                        ExpiresAt = DateTime.UtcNow.AddDays(30)
                    };
                }

                return (accessToken!, refreshToken, expiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token");
                throw;
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

        public async Task<UserInfoDto> GetUserInfoAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return await MapToUserInfoDtoAsync(user);
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
    }
} 
