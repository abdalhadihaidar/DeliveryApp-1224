using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using System.Linq;
using Volo.Abp.Identity;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [Route("api/app/user")]
    [ApiController]
    public class UserController : AbpController
    {
        private readonly IUserAppService _userAppService;
        private readonly IdentityUserManager _userManager;
        private readonly IAuthService _authService;

        public UserController(IUserAppService userAppService, IdentityUserManager userManager, IAuthService authService)
        {
            _userAppService = userAppService;
            _userManager = userManager;
            _authService = authService;
        }

        [HttpGet("{id}/profile")]
        public async Task<UserProfileDto> GetProfile(Guid id)
        {
            return await _userAppService.GetUserProfileAsync(id);
        }

        [HttpPut("profile")]
        public async Task<UserProfileDto> UpdateProfile([FromBody] UpdateUserProfileDto input)
        {
            // Extract userId from the current user context or from the input
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null; // Handle unauthorized
            }
            return await _userAppService.UpdateUserProfileAsync(userGuid, input);
        }

        [HttpGet("{userId}/addresses")]
        public async Task<List<AddressDto>> GetUserAddresses(Guid userId)
        {
            return await _userAppService.GetUserAddressesAsync(userId);
        }

        [HttpPost("addresses")]
        public async Task<AddressDto> AddAddress([FromBody] CreateAddressDto input)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null; // Handle unauthorized
            }
            return await _userAppService.AddAddressAsync(userGuid, input);
        }

        [HttpPut("addresses/{id}")]
        public async Task<AddressDto> UpdateAddress(Guid id, [FromBody] UpdateAddressDto input)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null; // Handle unauthorized
            }
            return await _userAppService.UpdateAddressAsync(userGuid, id, input);
        }

        [HttpDelete("addresses/{id}")]
        public async Task DeleteAddress(Guid id)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return; // Handle unauthorized
            }
            await _userAppService.DeleteAddressAsync(userGuid, id);
        }

        [HttpGet("{userId}/payment-methods")]
        public async Task<List<PaymentMethodDto>> GetUserPaymentMethods(Guid userId)
        {
            return await _userAppService.GetUserPaymentMethodsAsync(userId);
        }

        [HttpPost("payment-methods")]
        public async Task<PaymentMethodDto> AddPaymentMethod([FromBody] CreatePaymentMethodDto input)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null; // Handle unauthorized
            }
            return await _userAppService.AddPaymentMethodAsync(userGuid, input);
        }

        [HttpGet("{userId}/favorite-restaurants")]
        public async Task<List<Guid>> GetFavoriteRestaurants(Guid userId)
        {
            return await _userAppService.GetFavoriteRestaurantsAsync(userId);
        }

        [HttpPost("{userId}/favorite-restaurants/{restaurantId}")]
        public async Task<bool> AddFavoriteRestaurant(Guid userId, Guid restaurantId)
        {
            return await _userAppService.AddFavoriteRestaurantAsync(userId, restaurantId);
        }

        [HttpDelete("{userId}/favorite-restaurants/{restaurantId}")]
        public async Task<bool> RemoveFavoriteRestaurant(Guid userId, Guid restaurantId)
        {
            return await _userAppService.RemoveFavoriteRestaurantAsync(userId, restaurantId);
        }

        [HttpGet("{userId}/favorite-restaurants/{restaurantId}/check")]
        public async Task<bool> IsFavoriteRestaurant(Guid userId, Guid restaurantId)
        {
            return await _userAppService.IsFavoriteRestaurantAsync(userId, restaurantId);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Use AuthService for proper authentication with OpenIddict tokens
            var loginRequest = new LoginWithEmailDto
            {
                Email = loginDto.EmailOrPhone,
                Password = loginDto.Password,
                RememberMe = loginDto.RememberMe
            };

            var authResult = await _authService.LoginWithEmailAsync(loginRequest);
            
            if (!authResult.Success)
            {
                return Unauthorized(new { message = authResult.Message, errorCode = authResult.ErrorCode });
            }

            // For the dashboard we also want to send back the user object and an expiry (in seconds)
            var expiresIn = authResult.ExpiresAt.HasValue 
                ? (int)(authResult.ExpiresAt.Value - DateTime.UtcNow).TotalSeconds 
                : 3600;

            return Ok(new
            {
                token = authResult.AccessToken, // camelCase name as expected by the Angular client
                user = authResult.User,  // the validated user entity
                expiresIn
            });
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<PagedResultDto<UserDto>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _userAppService.GetListAsync(input);
        }

        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> CreateAsync([FromBody] CreateUserDto input)
        {
            return await _userAppService.CreateAsync(input);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> UpdateAsync(Guid id, [FromBody] UpdateUserDto input)
        {
            return await _userAppService.UpdateAsync(id, input);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task DeleteAsync(Guid id)
        {
            await _userAppService.DeleteAsync(id);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "admin,manager")]
        public async Task<List<string>> GetRoles()
        {
            return await _userAppService.GetAllRolesAsync();
        }

        [HttpPost("{id}/accept")]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> AcceptUserAsync(Guid id, [FromBody] string reason)
        {
            return await _userAppService.AcceptUserAsync(id, reason);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> RejectUserAsync(Guid id, [FromBody] string reason)
        {
            return await _userAppService.RejectUserAsync(id, reason);
        }

        [HttpGet("debug/admin-check")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugAdminCheck()
        {
            try
            {
                var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
                if (adminUser == null)
                {
                    return Ok(new { 
                        exists = false, 
                        message = "Admin user not found" 
                    });
                }

                var roles = await _userManager.GetRolesAsync(adminUser);
                var isInAdminRole = await _userManager.IsInRoleAsync(adminUser, "admin");
                var isInManagerRole = await _userManager.IsInRoleAsync(adminUser, "manager");

                return Ok(new { 
                    exists = true,
                    userId = adminUser.Id,
                    email = adminUser.Email,
                    emailConfirmed = adminUser.EmailConfirmed,
                    roles = roles,
                    isInAdminRole = isInAdminRole,
                    isInManagerRole = isInManagerRole,
                    message = "Admin user found"
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    exists = false, 
                    error = ex.Message,
                    message = "Error checking admin user" 
                });
            }
        }

        [HttpGet("debug/token-info")]
        [Authorize]
        public async Task<IActionResult> DebugTokenInfo()
        {
            try
            {
                var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

                var user = await _userManager.FindByEmailAsync(email);
                var userRoles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

                return Ok(new { 
                    userId = userId,
                    email = email,
                    tokenRoles = roles,
                    userRoles = userRoles,
                    claims = claims,
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    authenticationType = User.Identity?.AuthenticationType
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    error = ex.Message,
                    message = "Error checking token info" 
                });
            }
        }

        [HttpGet("debug/permissions")]
        [Authorize]
        public async Task<IActionResult> DebugPermissions()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userManager.FindByEmailAsync(email);
                
                if (user == null)
                {
                    return Ok(new { 
                        error = "User not found",
                        message = "Error checking permissions" 
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = new List<string>();

                // Check specific permissions
                var permissionChecks = new[]
                {
                    "AbpIdentity.Users",
                    "AbpIdentity.Users.Create",
                    "AbpIdentity.Users.Update",
                    "AbpIdentity.Users.Delete",
                    "AbpIdentity.Roles",
                    "AbpIdentity.Roles.Create",
                    "AbpIdentity.Roles.Update",
                    "AbpIdentity.Roles.Delete"
                };

                // For now, we'll just return the roles since permission checking requires additional services
                return Ok(new { 
                    userId = user.Id,
                    email = user.Email,
                    roles = roles,
                    permissionChecks = permissionChecks,
                    message = "Permission check completed (roles only)"
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    error = ex.Message,
                    message = "Error checking permissions" 
                });
            }
        }
    }
}
