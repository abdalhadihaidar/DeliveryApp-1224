using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Web.Controllers
{
    /// <summary>
    /// Registration API controller supporting email and phone authentication
    /// </summary>
    [ApiController]
    [Route("api/app/auth")]
    public class RegistrationController : AbpControllerBase
    {
        private readonly IAuthService _authService;

        public RegistrationController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register user with email
        /// </summary>
        /// <param name="request">Email registration request</param>
        /// <returns>Registration result</returns>
        [HttpPost("register/email")]
        [AllowAnonymous]
        public async Task<AuthResultDto> RegisterWithEmailAsync([FromBody] RegisterWithEmailDto request)
        {
            return await _authService.RegisterWithEmailAsync(request);
        }

        /// <summary>
        /// Register user with phone number
        /// </summary>
        /// <param name="request">Phone registration request</param>
        /// <returns>Registration result</returns>
        [HttpPost("register/phone")]
        [AllowAnonymous]
        public async Task<AuthResultDto> RegisterWithPhoneAsync([FromBody] RegisterWithPhoneDto request)
        {
            return await _authService.RegisterWithPhoneAsync(request);
        }

        /// <summary>
        /// Login with email
        /// </summary>
        /// <param name="request">Email login request</param>
        /// <returns>Login result</returns>
        [HttpPost("login/email")]
        [AllowAnonymous]
        public async Task<AuthResultDto> LoginWithEmailAsync([FromBody] LoginWithEmailDto request)
        {
            return await _authService.LoginWithEmailAsync(request);
        }

        /// <summary>
        /// Login with phone number
        /// </summary>
        /// <param name="request">Phone login request</param>
        /// <returns>Login result</returns>
        [HttpPost("login/phone")]
        [AllowAnonymous]
        public async Task<AuthResultDto> LoginWithPhoneAsync([FromBody] LoginWithPhoneDto request)
        {
            return await _authService.LoginWithPhoneAsync(request);
        }

        /// <summary>
        /// Verify email address
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify/email")]
        [AllowAnonymous]
        public async Task<AuthResultDto> VerifyEmailAsync([FromBody] VerifyEmailDto request)
        {
            return await _authService.VerifyEmailAsync(request);
        }

        /// <summary>
        /// Verify phone number
        /// </summary>
        /// <param name="request">Phone verification request</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify/phone")]
        [AllowAnonymous]
        public async Task<AuthResultDto> VerifyPhoneAsync([FromBody] VerifyPhoneDto request)
        {
            return await _authService.VerifyPhoneAsync(request);
        }

        /// <summary>
        /// Resend verification code
        /// </summary>
        /// <param name="request">Resend verification request</param>
        /// <returns>Resend result</returns>
        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<AuthResultDto> ResendVerificationCodeAsync([FromBody] ResendVerificationCodeDto request)
        {
            return await _authService.ResendVerificationCodeAsync(request);
        }

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="request">Password reset request</param>
        /// <returns>Reset result</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<AuthResultDto> ResetPasswordAsync([FromBody] ResetPasswordDto request)
        {
            return await _authService.ResetPasswordAsync(request);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Change password request</param>
        /// <returns>Change result</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<AuthResultDto> ChangePasswordAsync([FromBody] ChangePasswordDto request)
        {
            return await _authService.ChangePasswordAsync(request);
        }

        /// <summary>
        /// Refresh authentication token
        /// </summary>
        /// <param name="request">Token refresh request</param>
        /// <returns>Refresh result</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<AuthResultDto> RefreshTokenAsync([FromBody] RefreshTokenDto request)
        {
            return await _authService.RefreshTokenAsync(request);
        }

        /// <summary>
        /// Logout user
        /// </summary>
        /// <param name="request">Logout request</param>
        /// <returns>Logout result</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<AuthResultDto> LogoutAsync([FromBody] LogoutDto request)
        {
            return await _authService.LogoutAsync(request);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>Current user info</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            try
            {
                // This would typically get user info from the current context
                // For now, return a simple response
                var userId = User?.FindFirst("sub")?.Value ?? "unknown";
                return Ok(new { message = "User authenticated", userId = userId });
            }
            catch
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Check if email is available
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Availability result</returns>
        [HttpGet("check-email/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmailAvailabilityAsync(string email)
        {
            // This would check if email is already registered
            // For now, return a simple response
            return Ok(new { available = true, email });
        }

        /// <summary>
        /// Check if phone number is available
        /// </summary>
        /// <param name="phoneNumber">Phone number to check</param>
        /// <returns>Availability result</returns>
        [HttpGet("check-phone/{phoneNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckPhoneAvailabilityAsync(string phoneNumber)
        {
            // This would check if phone number is already registered
            // For now, return a simple response
            return Ok(new { available = true, phoneNumber });
        }
    }
}
