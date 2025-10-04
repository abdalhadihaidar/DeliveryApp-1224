using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using System.Security.Claims;
using System.Linq;
using Volo.Abp.Identity;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Volo.Abp.AspNetCore.Mvc;
using System.IO;

namespace DeliveryApp.Web.Controllers
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

        [HttpPut("addresses/{addressId}")]
        public async Task<AddressDto> UpdateAddress(Guid addressId, [FromBody] UpdateAddressDto input)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null; // Handle unauthorized
            }
            return await _userAppService.UpdateAddressAsync(userGuid, addressId, input);
        }

        [HttpDelete("addresses/{addressId}")]
        public async Task<bool> DeleteAddress(Guid addressId)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return false; // Handle unauthorized
            }
            return await _userAppService.DeleteAddressAsync(userGuid, addressId);
        }

        [HttpPut("addresses/{addressId}/default")]
        public async Task<AddressDto> SetDefaultAddress(Guid addressId)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null; // Handle unauthorized
            }
            return await _userAppService.SetDefaultAddressAsync(userGuid, addressId);
        }

        [HttpGet("{userId}/payment-methods")]
        public async Task<List<PaymentMethodDto>> GetPaymentMethods(Guid userId)
        {
            return await _userAppService.GetPaymentMethodsAsync(userId);
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

        [HttpDelete("payment-methods/{paymentMethodId}")]
        public async Task<bool> DeletePaymentMethod(Guid paymentMethodId)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return false; // Handle unauthorized
            }
            return await _userAppService.DeletePaymentMethodAsync(userGuid, paymentMethodId);
        }

        [HttpPut("payment-methods/{paymentMethodId}/default")]
        public async Task<PaymentMethodDto> SetDefaultPaymentMethod(Guid paymentMethodId)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null; // Handle unauthorized
            }
            return await _userAppService.SetDefaultPaymentMethodAsync(userGuid, paymentMethodId);
        }

        [HttpGet("{userId}/favorite-restaurants")]
        public async Task<List<Guid>> GetFavoriteRestaurants(Guid userId)
        {
            return await _userAppService.GetFavoriteRestaurantsAsync(userId);
        }

        [HttpPost("favorite-restaurants/{restaurantId}")]
        public async Task<bool> AddFavoriteRestaurant(Guid restaurantId)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return false; // Handle unauthorized
            }
            return await _userAppService.AddFavoriteRestaurantAsync(userGuid, restaurantId);
        }

        [HttpDelete("favorite-restaurants/{restaurantId}")]
        public async Task<bool> RemoveFavoriteRestaurant(Guid restaurantId)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return false; // Handle unauthorized
            }
            return await _userAppService.RemoveFavoriteRestaurantAsync(userGuid, restaurantId);
        }

        [HttpGet("favorite-restaurants/{restaurantId}/check")]
        public async Task<bool> IsFavoriteRestaurant(Guid restaurantId)
        {
            var userId = User.FindFirst("nameid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return false; // Handle unauthorized
            }
            return await _userAppService.IsFavoriteRestaurantAsync(userGuid, restaurantId);
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
        public async Task<Application.Contracts.Dtos.PagedResultDto<UserDto>> GetListAsync([FromQuery] Application.Contracts.Dtos.PagedAndSortedResultRequestDto input)
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
        public async Task<List<string>> GetRolesAsync()
        {
            return await _userAppService.GetAllRolesAsync();
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> DeactivateUserAsync(Guid id)
        {
            // Read the request body manually to handle text/plain content type
            string reason;
            using (var reader = new StreamReader(Request.Body))
            {
                reason = await reader.ReadToEndAsync();
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Deactivated by admin";
            }
            
            return await _userAppService.RejectUserAsync(id, reason);
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> ActivateUserAsync(Guid id)
        {
            // Read the request body manually to handle text/plain content type
            string reason;
            using (var reader = new StreamReader(Request.Body))
            {
                reason = await reader.ReadToEndAsync();
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Activated by admin";
            }
            
            return await _userAppService.AcceptUserAsync(id, reason);
        }

        [HttpPost("{id}/accept")]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> AcceptUserAsync(Guid id)
        {
            // Read the request body manually to handle text/plain content type
            string reason;
            using (var reader = new StreamReader(Request.Body))
            {
                reason = await reader.ReadToEndAsync();
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Accepted by admin";
            }
            
            return await _userAppService.AcceptUserAsync(id, reason);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "admin,manager")]
        public async Task<UserDto> RejectUserAsync(Guid id)
        {
            // Read the request body manually to handle text/plain content type
            string reason;
            using (var reader = new StreamReader(Request.Body))
            {
                reason = await reader.ReadToEndAsync();
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Rejected by admin";
            }
            
            return await _userAppService.RejectUserAsync(id, reason);
        }
    }
}
