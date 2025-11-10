using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;
using Volo.Abp.Identity;
using Volo.Abp.Domain.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Data;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Secure authentication service with transaction management
    /// Ensures data consistency during user registration and authentication
    /// </summary>
    public class SecureAuthService : DomainService, ISecureAuthService
    {
        private readonly IdentityUserManager _userManager;
        private readonly ILogger<SecureAuthService> _logger;
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;
        private readonly TransactionManagementService _transactionService;
        private readonly IEmailService _emailService;

        public SecureAuthService(
            IdentityUserManager userManager,
            ILogger<SecureAuthService> logger,
            IRepository<Restaurant, Guid> restaurantRepository,
            IUserRepository userRepository,
            IStringLocalizer<DeliveryAppResource> localizer,
            TransactionManagementService transactionService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _logger = logger;
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
            _localizer = localizer;
            _transactionService = transactionService;
            _emailService = emailService;
        }

        /// <summary>
        /// Secure user registration with transaction management
        /// Ensures all operations succeed or all are rolled back
        /// </summary>
        public async Task<AuthResultDto> RegisterWithEmailSecureAsync(RegisterWithEmailDto request)
        {
            return await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation("Starting secure registration for email: {Email}, Role: {Role}", 
                    request.Email, request.Role);

                // Step 1: Validate user doesn't exist
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

                // Step 2: Create user
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

                user.SetProperty("PreferredLanguage", request.Language);
                user.ProfileImageUrl = string.Empty;
                user.ReviewStatus = DeliveryApp.Domain.Enums.ReviewStatus.Pending;
                user.ReviewReason = null;

                // Step 3: Create user in database
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

                // Step 4: Assign role
                var roleName = MapRoleToRoleName(request.Role);
                await _userManager.AddToRoleAsync(user, roleName);

                // Step 5: Create default restaurant if restaurant owner
                if (request.Role == UserRole.RestaurantOwner)
                {
                    await CreateDefaultRestaurantForOwnerSecureAsync(user.Id);
                }

                // Step 6: Send verification email (non-critical, can fail without rolling back)
                EmailSendResultDto emailResult = null;
                try
                {
                    emailResult = await _emailService.SendVerificationEmailAsync(new SendVerificationEmailDto
                    {
                        Email = request.Email,
                        UserName = $"{request.FirstName} {request.LastName}",
                        Language = request.Language
                    });

                    if (emailResult?.Success == true)
                    {
                        _logger.LogInformation("Verification email sent successfully to {Email}", request.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send verification email to {Email}: {Message}", 
                            request.Email, emailResult?.Message ?? "Unknown error");
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send verification email for user: {Email}", request.Email);
                    // Don't fail registration if email fails
                }

                _logger.LogInformation("User registered successfully with email: {Email}", request.Email);

                return new AuthResultDto
                {
                    Success = true,
                    Message = emailResult?.Success == true
                        ? "Registration successful. Please verify your email."
                        : "Registration successful. Please check your email for verification code. If you didn't receive it, you can request a resend.",
                    RequiresVerification = true,
                    VerificationType = "Email"
                };
            }, $"UserRegistration_{request.Email}");
        }

        /// <summary>
        /// Secure restaurant creation with transaction management
        /// </summary>
        private async Task CreateDefaultRestaurantForOwnerSecureAsync(Guid userId)
        {
            await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                // Check if restaurant already exists
                var existingRestaurant = await _restaurantRepository.FirstOrDefaultAsync(r => r.OwnerId == userId);
                if (existingRestaurant == null)
                {
                    var restaurant = new Restaurant(GuidGenerator.Create())
                    {
                        OwnerId = userId,
                        Name = "My Restaurant",
                        Description = "Welcome to my restaurant",
                        Address = new Address
                        {
                            Street = "",
                            City = "",
                            State = "",
                            ZipCode = "",
                            FullAddress = ""
                        },
                        IsActive = true,
                        Rating = 0.0,
                        DeliveryFee = 0,
                        MinimumOrderAmount = 0,
                        DeliveryTime = "30-45 دقيقة"
                    };

                    await _restaurantRepository.InsertAsync(restaurant);
                    _logger.LogInformation("Created default restaurant for user {UserId}", userId);
                }
            }, $"CreateDefaultRestaurant_{userId}");
        }

        /// <summary>
        /// Secure user update with validation and transaction management
        /// </summary>
        public async Task<AuthResultDto> UpdateUserProfileSecureAsync(Guid userId, SecureUpdateUserProfileDto request)
        {
            return await _transactionService.ExecuteWithValidationAsync(
                // Validation: Check if user exists and is active
                async () =>
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    return user != null && user.IsActive;
                },
                // Main operation: Update user profile
                async () =>
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user == null)
                    {
                        return new AuthResultDto
                        {
                            Success = false,
                            Message = "User not found",
                            ErrorCode = "USER_NOT_FOUND"
                        };
                    }

                    // Update user properties
                    if (!string.IsNullOrEmpty(request.FirstName))
                        user.Name = request.FirstName;
                    
                    if (!string.IsNullOrEmpty(request.LastName))
                        user.Surname = request.LastName;
                    
                    if (!string.IsNullOrEmpty(request.PhoneNumber))
                        user.SetPhoneNumber(request.PhoneNumber, false);
                    
                    if (!string.IsNullOrEmpty(request.Language))
                        user.SetProperty("PreferredLanguage", request.Language);

                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        return new AuthResultDto
                        {
                            Success = false,
                            Message = "Failed to update user profile",
                            ErrorCode = "UPDATE_FAILED"
                        };
                    }

                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Profile updated successfully"
                    };
                },
                $"UpdateUserProfile_{userId}"
            );
        }

        /// <summary>
        /// Secure password change with transaction management
        /// </summary>
        public async Task<AuthResultDto> ChangePasswordSecureAsync(Guid userId, SecureChangePasswordDto request)
        {
            return await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                // Verify current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Current password is incorrect",
                        ErrorCode = "INVALID_CURRENT_PASSWORD"
                    };
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                        ErrorCode = "PASSWORD_CHANGE_FAILED"
                    };
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

                return new AuthResultDto
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }, $"ChangePassword_{userId}");
        }

        /// <summary>
        /// Secure account deletion with cleanup
        /// </summary>
        public async Task<AuthResultDto> DeleteAccountSecureAsync(Guid userId, string password)
        {
            return await _transactionService.ExecuteWithCompensationAsync(
                // Main operation: Delete account
                async () =>
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user == null)
                    {
                        return new AuthResultDto
                        {
                            Success = false,
                            Message = "User not found",
                            ErrorCode = "USER_NOT_FOUND"
                        };
                    }

                    // Verify password
                    var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
                    if (!isPasswordValid)
                    {
                        return new AuthResultDto
                        {
                            Success = false,
                            Message = "Invalid password",
                            ErrorCode = "INVALID_PASSWORD"
                        };
                    }

                    // Soft delete user
                    user.SetIsActive(false);
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("Account deleted successfully for user: {UserId}", userId);

                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Account deleted successfully"
                    };
                },
                // Compensation: Reactivate account if deletion fails
                async () =>
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user != null)
                    {
                        user.SetIsActive(true);
                        await _userManager.UpdateAsync(user);
                        _logger.LogInformation("Account reactivated due to deletion failure: {UserId}", userId);
                    }
                },
                $"DeleteAccount_{userId}"
            );
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
    }
}
