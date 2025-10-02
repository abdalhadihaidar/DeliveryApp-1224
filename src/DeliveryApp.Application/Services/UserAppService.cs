using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.Application.Services;
using Volo.Abp;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Repositories;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Microsoft.EntityFrameworkCore;

using DeliveryApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using DeliveryApp.Services;
using Volo.Abp.Data;
using Volo.Abp.Users;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Services
{
    public class UserAppService : ApplicationService, IUserAppService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IdentityRoleManager _roleManager;
        private readonly INotificationService _notificationService;
        private readonly IFirebaseNotificationService _firebaseNotificationService;

        public UserAppService(
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository,
            IdentityUserManager userManager,
            IdentityRoleManager roleManager,
            INotificationService notificationService,
            IFirebaseNotificationService firebaseNotificationService)
        {
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationService = notificationService;
            _firebaseNotificationService = firebaseNotificationService;
        }

                public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithDetailsAsync(userId);
            var addresses = await _userRepository.GetUserAddressesAsync(userId);
            var favoriteRestaurants = await _userRepository.GetUserFavoriteRestaurantsAsync(userId);
            var paymentMethods = await _userRepository.GetUserPaymentMethodsAsync(userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                Addresses = addresses.Select(a => new AddressDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Street = a.Street,
                    City = a.City,
                    State = a.State,
                    ZipCode = a.ZipCode,
                    FullAddress = a.FullAddress,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    IsDefault = a.IsDefault
                }).ToList(),
                FavoriteRestaurants = favoriteRestaurants,
                PaymentMethods = paymentMethods.Select(p => new PaymentMethodDto
                {
                    Id = p.Id,
                    Type = p.Type,
                    Title = p.Title,
                    LastFourDigits = p.LastFourDigits,
                    CardHolderName = p.CardHolderName,
                    ExpiryDate = p.ExpiryDate,
                    IsDefault = p.IsDefault
                }).ToList()
            };
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto input)
        {
            var user = await _userRepository.GetAsync(userId);

            if (!string.IsNullOrEmpty(input.Name))
                user.Name = input.Name;
            
            if (!string.IsNullOrEmpty(input.ProfileImageUrl))
                user.ProfileImageUrl = input.ProfileImageUrl;

            await _userRepository.UpdateAsync(user);

            // Return updated profile
            return await GetUserProfileAsync(userId);
        }

        public async Task<List<AddressDto>> GetUserAddressesAsync(Guid userId)
        {
            var addresses = await _userRepository.GetUserAddressesAsync(userId);
            return addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                Title = a.Title,
                Street = a.Street,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                FullAddress = a.FullAddress,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                IsDefault = a.IsDefault
            }).ToList();
        }

        public async Task<AddressDto> AddAddressAsync(Guid userId, CreateAddressDto input)
        {
            var address = new Domain.Entities.Address
            {
                UserId = input.UserId,
                Title = input.Title,
                Street = input.Street,
                City = input.City,
                State = input.State,
                ZipCode = input.ZipCode,
                FullAddress = input.FullAddress,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                IsDefault = input.IsDefault
            };

            // If this is the default address, unset any existing default
            if (address.IsDefault)
            {
                await _userRepository.UnsetDefaultAddressAsync(userId);
            }

            await _userRepository.AddAddressAsync(address);

            return new AddressDto
            {
                Id = address.Id,
                Title = address.Title,
                Street = address.Street,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                FullAddress = address.FullAddress,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                IsDefault = address.IsDefault
            };
        }

        public async Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto input)
        {
            var addresses = await _userRepository.GetUserAddressesAsync(userId);
            var address = addresses.FirstOrDefault(a => a.Id == addressId);

            if (address == null)
            {
                throw new Exception("Address not found");
            }

            address.Title = input.Title;
            address.Street = input.Street;
            address.City = input.City;
            address.State = input.State;
            address.ZipCode = input.ZipCode;
            address.FullAddress = input.FullAddress;
            address.Latitude = input.Latitude;
            address.Longitude = input.Longitude;

            // If this is being set as default, unset any existing default
            if (input.IsDefault && !address.IsDefault)
            {
                await _userRepository.UnsetDefaultAddressAsync(userId);
                address.IsDefault = true;
            }

            await _userRepository.UpdateAddressAsync(address);

            return new AddressDto
            {
                Id = address.Id,
                Title = address.Title,
                Street = address.Street,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                FullAddress = address.FullAddress,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                IsDefault = address.IsDefault
            };
        }

        public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId)
        {
            var addresses = await _userRepository.GetUserAddressesAsync(userId);
            var address = addresses.FirstOrDefault(a => a.Id == addressId);

            if (address == null)
            {
                throw new Exception("Address not found");
            }

            await _userRepository.DeleteAddressAsync(address);

            return true;
        }

        public async Task<AddressDto> SetDefaultAddressAsync(Guid userId, Guid addressId)
        {
            var addresses = await _userRepository.GetUserAddressesAsync(userId);
            var address = addresses.FirstOrDefault(a => a.Id == addressId);

            if (address == null)
            {
                throw new Exception("Address not found");
            }

            // Unset any existing default
            await _userRepository.UnsetDefaultAddressAsync(userId);

            // Set this address as default
            address.IsDefault = true;
            await _userRepository.UpdateAddressAsync(address);

            return new AddressDto
            {
                Id = address.Id,
                Title = address.Title,
                Street = address.Street,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                FullAddress = address.FullAddress,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                IsDefault = address.IsDefault
            };
        }

        public async Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(Guid userId)
        {
            var paymentMethods = await _userRepository.GetUserPaymentMethodsAsync(userId);
            return paymentMethods.Select(p => new PaymentMethodDto
            {
                Id = p.Id,
                Type = p.Type,
                Title = p.Title,
                LastFourDigits = p.LastFourDigits,
                CardHolderName = p.CardHolderName,
                ExpiryDate = p.ExpiryDate,
                IsDefault = p.IsDefault
            }).ToList();
        }

        public async Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(Guid userId)
        {
            return await GetPaymentMethodsAsync(userId);
        }

        public async Task<PaymentMethodDto> AddPaymentMethodAsync(Guid userId, CreatePaymentMethodDto input)
        {
            var paymentMethod = new Domain.Entities.PaymentMethod
            {
                UserId = input.UserId,
                Type = input.Type,
                Title = input.Title,
                LastFourDigits = input.LastFourDigits,
                CardHolderName = input.CardHolderName,
                ExpiryDate = input.ExpiryDate,
                IsDefault = input.IsDefault
            };

            // If this is the default payment method, unset any existing default
            if (paymentMethod.IsDefault)
            {
                await _userRepository.UnsetDefaultPaymentMethodAsync(userId);
            }

            await _userRepository.AddPaymentMethodAsync(paymentMethod);

            return new PaymentMethodDto
            {
                Id = paymentMethod.Id,
                Type = paymentMethod.Type,
                Title = paymentMethod.Title,
                LastFourDigits = paymentMethod.LastFourDigits,
                CardHolderName = paymentMethod.CardHolderName,
                ExpiryDate = paymentMethod.ExpiryDate,
                IsDefault = paymentMethod.IsDefault
            };
        }

        public async Task<bool> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId)
        {
            var paymentMethods = await _userRepository.GetUserPaymentMethodsAsync(userId);
            var paymentMethod = paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);

            if (paymentMethod == null)
            {
                throw new Exception("Payment method not found");
            }

            await _userRepository.DeletePaymentMethodAsync(paymentMethod);

            return true;
        }

        public async Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId)
        {
            var paymentMethods = await _userRepository.GetUserPaymentMethodsAsync(userId);
            var paymentMethod = paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);

            if (paymentMethod == null)
            {
                throw new Exception("Payment method not found");
            }

            // Unset any existing default
            await _userRepository.UnsetDefaultPaymentMethodAsync(userId);

            // Set this payment method as default
            paymentMethod.IsDefault = true;
            await _userRepository.UpdatePaymentMethodAsync(paymentMethod);

            return new PaymentMethodDto
            {
                Id = paymentMethod.Id,
                Type = paymentMethod.Type,
                Title = paymentMethod.Title,
                LastFourDigits = paymentMethod.LastFourDigits,
                CardHolderName = paymentMethod.CardHolderName,
                ExpiryDate = paymentMethod.ExpiryDate,
                IsDefault = paymentMethod.IsDefault
            };
        }

        public async Task<List<Guid>> GetFavoriteRestaurantsAsync(Guid userId)
        {
            return await _userRepository.GetUserFavoriteRestaurantsAsync(userId);
        }

        public async Task<bool> AddFavoriteRestaurantAsync(Guid userId, Guid restaurantId)
        {
            // Check if already a favorite
            var isFavorite = await _userRepository.IsFavoriteRestaurantAsync(userId, restaurantId);

            if (isFavorite)
            {
                return true; // Already a favorite
            }

            await _userRepository.AddFavoriteRestaurantAsync(userId, restaurantId);

            return true;
        }

        public async Task<bool> RemoveFavoriteRestaurantAsync(Guid userId, Guid restaurantId)
        {
            // Check if it's a favorite
            var isFavorite = await _userRepository.IsFavoriteRestaurantAsync(userId, restaurantId);

            if (!isFavorite)
            {
                return true; // Not a favorite, nothing to do
            }

            await _userRepository.RemoveFavoriteRestaurantAsync(userId, restaurantId);

            return true;
        }

        public async Task<bool> IsFavoriteRestaurantAsync(Guid userId, Guid restaurantId)
        {
            return await _userRepository.IsFavoriteRestaurantAsync(userId, restaurantId);
        }

        // Authentication methods
        public async Task<UserProfileDto> ValidateUserAsync(LoginDto loginDto)
        {
            // Find user by email or phone number
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrPhone);
            
            // If not found by email, try by phone number
            if (user == null)
            {
                // For phone number lookup, we need to query users by phone number
                // This is a simplified approach - in a real app you'd have a proper phone number index
                var users = await _userManager.Users.ToListAsync();
                user = users.FirstOrDefault(u => u.PhoneNumber == loginDto.EmailOrPhone);
            }

            if (user == null)
            {
                return null;
            }

            // Verify password
            var isValidPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isValidPassword)
            {
                return null;
            }

            // Map to UserProfileDto - return a simplified version for login
            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = "" // Will be populated when getting full profile
            };
        }

        // NOTE: JWT token generation removed - now using OpenIddict consistently
        // Use AuthService or MobileAuthService for token generation

        public async Task<List<string>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(r => r.Name).ToList();
        }

        public async Task<PagedResultDto<UserDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var users = await _userRepository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, input.Sorting, null);
            var totalCount = await _userRepository.GetCountAsync(null);
            var userDtos = new List<UserDto>();
            
            foreach (var appUser in users)
            {
                // Get roles from IdentityUser
                var identityUser = await _userManager.FindByIdAsync(appUser.Id.ToString());
                var roles = identityUser != null ? (await _userManager.GetRolesAsync(identityUser)).ToList() : new List<string>();
                var primaryRole = roles.FirstOrDefault();
                
                userDtos.Add(new UserDto
                {
                    Id = appUser.Id,
                    UserName = appUser.UserName,
                    Email = appUser.Email,
                    Name = appUser.Name,
                    PhoneNumber = appUser.PhoneNumber,
                    ProfileImageUrl = appUser.ProfileImageUrl,
                    Role = MapRoleStringToEnum(primaryRole),
                    Roles = roles,
                    IsActive = appUser.IsActive,
                    ReviewStatus = appUser.ReviewStatus.ToString(),
                    ReviewReason = appUser.ReviewReason,
                    CreationTime = appUser.CreationTime,
                    UserType = primaryRole
                });
            }
            return new PagedResultDto<UserDto>(totalCount, userDtos);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            var user = new AppUser(Guid.NewGuid(), input.Email, input.Email)
            {
                Name = input.Name,
                ProfileImageUrl = input.ProfileImageUrl ?? string.Empty
            };
            var identityResult = await _userManager.CreateAsync(user, input.Password);
            if (!identityResult.Succeeded)
            {
                throw new Exception(string.Join("; ", identityResult.Errors.Select(e => e.Description)));
            }
            await _userManager.SetUserNameAsync(user, input.Email);
            if (!string.IsNullOrEmpty(input.PhoneNumber))
                await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
            if (!string.IsNullOrEmpty(input.Role))
            {
                await _userManager.AddToRoleAsync(user, input.Role);
            }
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = MapRoleStringToEnum(input.Role),
                Roles = roles,
                IsActive = user.IsActive,
                ReviewStatus = user.ReviewStatus.ToString(),
                ReviewReason = user.ReviewReason,
                CreationTime = user.CreationTime,
                UserType = input.Role
            };
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input)
        {
            // Get the AppUser with additional properties
            var appUser = await _userRepository.GetAsync(id);
            if (appUser == null)
            {
                throw new Exception("User not found");
            }

            // Get the IdentityUser for role management
            var identityUser = await _userManager.FindByIdAsync(id.ToString());
            if (identityUser == null)
            {
                throw new Exception("Identity user not found");
            }

            // Update AppUser properties
            appUser.Name = input.Name;
            appUser.ProfileImageUrl = input.ProfileImageUrl ?? string.Empty;
            await _userRepository.UpdateAsync(appUser);

            // Update IdentityUser properties
            if (!string.IsNullOrEmpty(input.PhoneNumber))
                await _userManager.SetPhoneNumberAsync(identityUser, input.PhoneNumber);

            var identityResult = await _userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
            {
                throw new Exception(string.Join("; ", identityResult.Errors.Select(e => e.Description)));
            }

            // Update roles if provided
            if (!string.IsNullOrEmpty(input.Role))
            {
                var currentRoles = (await _userManager.GetRolesAsync(identityUser)).ToList();
                if (!currentRoles.Contains(input.Role) || currentRoles.Count > 1)
                {
                    await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                    await _userManager.AddToRoleAsync(identityUser, input.Role);
                }
            }

            var roles = (await _userManager.GetRolesAsync(identityUser)).ToList();
            return new UserDto
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email,
                Name = appUser.Name,
                PhoneNumber = appUser.PhoneNumber,
                ProfileImageUrl = appUser.ProfileImageUrl,
                Role = MapRoleStringToEnum(roles.FirstOrDefault()),
                Roles = roles,
                IsActive = appUser.IsActive,
                ReviewStatus = appUser.ReviewStatus.ToString(),
                ReviewReason = appUser.ReviewReason,
                CreationTime = appUser.CreationTime,
                UserType = roles.FirstOrDefault()
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            // Get the AppUser with additional properties
            var appUser = await _userRepository.GetAsync(id);
            if (appUser == null)
            {
                throw new Exception("User not found");
            }

            // Get the IdentityUser for role management
            var identityUser = await _userManager.FindByIdAsync(id.ToString());
            if (identityUser == null)
            {
                throw new Exception("Identity user not found");
            }

            // Soft delete AppUser (mark as deleted)
            appUser.IsDeleted = true;
            appUser.DeletionTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(appUser);

            // Soft delete IdentityUser (mark as deleted)
            identityUser.IsDeleted = true;
            var identityResult = await _userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
            {
                throw new Exception(string.Join("; ", identityResult.Errors.Select(e => e.Description)));
            }

            // Send notification to user about account deletion
            await _notificationService.SendGeneralNotificationAsync(
                appUser.Id.ToString(), 
                "Account Deleted", 
                "Your account has been deleted. If this was a mistake, please contact support."
            );
        }

        public async Task RestoreAsync(Guid id)
        {
            // Get the AppUser with additional properties
            var appUser = await _userRepository.GetAsync(id);
            if (appUser == null)
            {
                throw new Exception("User not found");
            }

            // Get the IdentityUser for role management
            var identityUser = await _userManager.FindByIdAsync(id.ToString());
            if (identityUser == null)
            {
                throw new Exception("Identity user not found");
            }

            // Restore AppUser (unmark as deleted)
            appUser.IsDeleted = false;
            appUser.DeletionTime = null;
            await _userRepository.UpdateAsync(appUser);

            // Restore IdentityUser (unmark as deleted)
            identityUser.IsDeleted = false;
            var identityResult = await _userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
            {
                throw new Exception(string.Join("; ", identityResult.Errors.Select(e => e.Description)));
            }

            // Send notification to user about account restoration
            await _notificationService.SendGeneralNotificationAsync(
                appUser.Id.ToString(), 
                "Account Restored", 
                "Your account has been restored. You can now log in again."
            );
        }

        public async Task<UserDto> AcceptUserAsync(Guid userId, string reason)
        {
            // First try to get the AppUser with additional properties
            var appUser = await _userRepository.GetAsync(userId);
            if (appUser == null)
            {
                throw new Exception($"User not found with ID: {userId}");
            }

            // Get the IdentityUser for role management
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
            {
                throw new Exception($"Identity user not found with ID: {userId}");
            }
            // Update AppUser properties

            appUser.ReviewStatus = DeliveryApp.Domain.Enums.ReviewStatus.Accepted;
            appUser.ReviewReason = reason;
            await _userRepository.UpdateAsync(appUser);

            // Update IdentityUser active status
            identityUser.SetIsActive(true);
            var result = await _userManager.UpdateAsync(identityUser);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            var roles = (await _userManager.GetRolesAsync(identityUser)).ToList();
            
            // Send in-app notification
            await _notificationService.SendGeneralNotificationAsync(appUser.Id.ToString(), "Account Approved", "Your account has been approved and activated. Welcome!");
            
            // Send push notification (replace deviceToken with real one in production)
            var deviceToken = appUser.GetProperty<string>("DeviceToken") ?? "";
            if (!string.IsNullOrEmpty(deviceToken))
            {
                await _firebaseNotificationService.SendNotificationAsync(deviceToken, new NotificationMessage
                {
                    Title = "Account Approved",
                    Body = "Your account has been approved and activated. Welcome!",
                    Priority = Contracts.Dtos.NotificationPriority.High
                });
            }
            
            return new UserDto
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email,
                Name = appUser.Name,
                PhoneNumber = appUser.PhoneNumber,
                ProfileImageUrl = appUser.ProfileImageUrl,
                Role = MapRoleStringToEnum(roles.FirstOrDefault()),
                Roles = roles,
                IsActive = appUser.IsActive,
                ReviewStatus = appUser.ReviewStatus.ToString(),
                ReviewReason = appUser.ReviewReason,
                CreationTime = appUser.CreationTime,
                UserType = roles.FirstOrDefault()
            };
        }

        public async Task<UserDto> RejectUserAsync(Guid userId, string reason)
        {
            // First try to get the AppUser with additional properties
            var appUser = await _userRepository.GetAsync(userId);
            if (appUser == null)
            {
                throw new Exception($"User not found with ID: {userId}");
            }

            // Get the IdentityUser for role management
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
            {
                throw new Exception($"Identity user not found with ID: {userId}");
            }
            // Update AppUser properties
            appUser.ReviewStatus = DeliveryApp.Domain.Enums.ReviewStatus.Rejected;
            appUser.ReviewReason = reason;
            await _userRepository.UpdateAsync(appUser);

            // Update IdentityUser active status
            identityUser.SetIsActive(false);
            var result = await _userManager.UpdateAsync(identityUser);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            var roles = (await _userManager.GetRolesAsync(identityUser)).ToList();
            
            // Send in-app notification
            await _notificationService.SendGeneralNotificationAsync(appUser.Id.ToString(), "Account Rejected", $"Your account was rejected. Reason: {reason}");
            
            // Send push notification (replace deviceToken with real one in production)
            var deviceToken = appUser.GetProperty<string>("DeviceToken") ?? "";
            if (!string.IsNullOrEmpty(deviceToken))
            {
                await _firebaseNotificationService.SendNotificationAsync(deviceToken, new NotificationMessage
                {
                    Title = "Account Rejected",
                    Body = $"Your account was rejected. Reason: {reason}",
                    Priority = Contracts.Dtos.NotificationPriority.High
                });
            }
            
            return new UserDto
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email,
                Name = appUser.Name,
                PhoneNumber = appUser.PhoneNumber,
                ProfileImageUrl = appUser.ProfileImageUrl,
                Role = MapRoleStringToEnum(roles.FirstOrDefault()),
                Roles = roles,
                IsActive = appUser.IsActive,
                ReviewStatus = appUser.ReviewStatus.ToString(),
                ReviewReason = appUser.ReviewReason,
                CreationTime = appUser.CreationTime,
                UserType = roles.FirstOrDefault()
            };
        }

        public async Task<List<RestaurantDto>> GetRestaurantsByOwnerAsync(Guid ownerId)
        {
            var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == ownerId);
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        public async Task<List<UserDto>> GetAllUsersForDebugAsync()
        {
            var users = await _userRepository.GetListAsync();
            var userDtos = new List<UserDto>();
            
            foreach (var user in users)
            {
                // Get roles from IdentityUser
                var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
                var roles = identityUser != null ? await _userManager.GetRolesAsync(identityUser) : new List<string>();
                var primaryRole = roles.FirstOrDefault() ?? "customer";
                
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Role = MapRoleStringToEnum(primaryRole),
                    Roles = (List<string>)roles,
                    IsActive = user.IsActive,
                    ReviewStatus = user.ReviewStatus.ToString(),
                    ReviewReason = user.ReviewReason,
                    CreationTime = user.CreationTime,
                    UserType = primaryRole
                });
            }
            
            return userDtos;
        }

        public async Task<List<UserDto>> GetSoftDeletedUsersForDebugAsync()
        {
            var dbContext = await _userRepository.GetDbContextAsync();
            
            // Query soft-deleted users from AbpUsers table
            var softDeletedUsers = await dbContext.Set<IdentityUser>()
                .Where(u => u.IsDeleted)
                .ToListAsync();
            
            var userDtos = new List<UserDto>();
            
            foreach (var identityUser in softDeletedUsers)
            {
                // Get AppUser data if available
                var appUser = await dbContext.Set<AppUser>()
                    .FirstOrDefaultAsync(u => u.Id == identityUser.Id);
                
                var roles = await _userManager.GetRolesAsync(identityUser);
                var primaryRole = roles.FirstOrDefault() ?? "customer";
                
                userDtos.Add(new UserDto
                {
                    Id = identityUser.Id,
                    UserName = identityUser.UserName,
                    Email = identityUser.Email,
                    Name = identityUser.Name,
                    PhoneNumber = identityUser.PhoneNumber,
                    ProfileImageUrl = appUser?.ProfileImageUrl ?? string.Empty,
                    Role = MapRoleStringToEnum(primaryRole),
                    Roles = (List<string>)roles,
                    IsActive = identityUser.IsActive,
                    ReviewStatus = appUser?.ReviewStatus.ToString(),
                    ReviewReason = appUser?.ReviewReason,
                    CreationTime = identityUser.CreationTime,
                    UserType = primaryRole
                });
            }
            
            return userDtos;
        }

        public async Task<UserDto?> GetUserByIdForDebugAsync(Guid userId)
        {
            // First try to get the AppUser with additional properties
            var appUser = await _userRepository.GetAsync(userId);
            if (appUser == null)
            {
                return null;
            }

            // Get the IdentityUser for role management
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            var primaryRole = roles.FirstOrDefault() ?? "customer";
            
            return new UserDto
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email,
                Name = appUser.Name,
                PhoneNumber = appUser.PhoneNumber,
                ProfileImageUrl = appUser.ProfileImageUrl,
                Role = MapRoleStringToEnum(primaryRole),
                Roles = (List<string>)roles,
                IsActive = appUser.IsActive,
                ReviewStatus = appUser.ReviewStatus.ToString(),
                ReviewReason = appUser.ReviewReason,
                CreationTime = appUser.CreationTime,
                UserType = primaryRole,
                LastLoginTime = null, // LastLoginTime is not available in IdentityUser
                EmailConfirmed = identityUser.EmailConfirmed,
                PhoneNumberConfirmed = identityUser.PhoneNumberConfirmed,
               
            };
        }

        public async Task<UserDto> GetAsync(Guid id)
        {
            // First try to get the AppUser with additional properties
            var appUser = await _userRepository.GetAsync(id);
            if (appUser == null)
            {
                throw new UserFriendlyException($"User with ID {id} not found.");
            }

            // Get the IdentityUser for role management
            var identityUser = await _userManager.FindByIdAsync(id.ToString());
            if (identityUser == null)
            {
                throw new UserFriendlyException($"Identity user with ID {id} not found.");
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            var primaryRole = roles.FirstOrDefault() ?? "customer";
            
            return new UserDto
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email,
                Name = appUser.Name,
                PhoneNumber = appUser.PhoneNumber,
                ProfileImageUrl = appUser.ProfileImageUrl,
                Role = MapRoleStringToEnum(primaryRole),
                Roles = (List<string>)roles,
                IsActive = appUser.IsActive,
                ReviewStatus = appUser.ReviewStatus.ToString(),
                ReviewReason = appUser.ReviewReason,
                CreationTime = appUser.CreationTime,
                UserType = primaryRole,
                LastLoginTime = null, // LastLoginTime is not available in IdentityUser
                EmailConfirmed = identityUser.EmailConfirmed,
                PhoneNumberConfirmed = identityUser.PhoneNumberConfirmed,
             
            };
        }

        private UserRole MapRoleStringToEnum(string? roleString)
        {
            if (string.IsNullOrWhiteSpace(roleString))
            {
                // Default to Customer role if no role is assigned
                return UserRole.Customer;
            }
            
            return roleString.ToLower() switch
            {
                "customer" => UserRole.Customer,
                "delivery" => UserRole.Delivery,
                "restaurant_owner" => UserRole.RestaurantOwner,
                "admin" => UserRole.RestaurantOwner, // Map admin to RestaurantOwner for user management
                "manager" => UserRole.RestaurantOwner, // Map manager to RestaurantOwner for user management
                _ => UserRole.Customer // Default to Customer for unknown roles
            };
        }
    }
}

