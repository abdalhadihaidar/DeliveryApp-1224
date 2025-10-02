using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.Extensions.Options;
using DeliveryApp.Application.Services;
using System;
using System.Linq;

namespace DeliveryApp.Web.Controllers
{
    /// <summary>
    /// Mobile-specific user API controller using JWT tokens
    /// </summary>
    [ApiController]
    [Route("api/app/mobile-user")]
    public class MobileUserController : ControllerBase
    {
        private readonly IMobileAuthService _mobileAuthService;
        private readonly JwtSettings _jwtSettings;

        public MobileUserController(
            IMobileAuthService mobileAuthService,
            IOptions<JwtSettings> jwtSettings)
        {
            _mobileAuthService = mobileAuthService;
            _jwtSettings = jwtSettings.Value;
        }

        /// <summary>
        /// Get current user info (mobile)
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<UserInfoDto> GetCurrentUserAsync()
        {
            // Extract user ID from OpenIddict token - try multiple claim types
            var userId = User.FindFirst("sub")?.Value ?? 
                        User.FindFirst("nameid")?.Value ?? 
                        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        User.FindFirst("user_id")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                // Log available claims for debugging
                var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                throw new UnauthorizedAccessException($"Invalid token - no user ID found. Available claims: {string.Join(", ", claims)}");
            }

            // Get user details from database using the mobile auth service
            var userInfo = await _mobileAuthService.GetUserInfoAsync(userId);
            
            return userInfo;
        }
    }
}
