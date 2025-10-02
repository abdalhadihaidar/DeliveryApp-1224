using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Enhanced authentication service interface supporting email and phone authentication
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Register user with email
        /// </summary>
        /// <param name="request">Email registration request</param>
        /// <returns>Registration result</returns>
        Task<AuthResultDto> RegisterWithEmailAsync(RegisterWithEmailDto request);

        /// <summary>
        /// Register user with phone number
        /// </summary>
        /// <param name="request">Phone registration request</param>
        /// <returns>Registration result</returns>
        Task<AuthResultDto> RegisterWithPhoneAsync(RegisterWithPhoneDto request);

        /// <summary>
        /// Login with email
        /// </summary>
        /// <param name="request">Email login request</param>
        /// <returns>Login result</returns>
        Task<AuthResultDto> LoginWithEmailAsync(LoginWithEmailDto request);

        /// <summary>
        /// Login with phone number
        /// </summary>
        /// <param name="request">Phone login request</param>
        /// <returns>Login result</returns>
        Task<AuthResultDto> LoginWithPhoneAsync(LoginWithPhoneDto request);

        /// <summary>
        /// Verify email address
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Verification result</returns>
        Task<AuthResultDto> VerifyEmailAsync(VerifyEmailDto request);

        /// <summary>
        /// Verify phone number
        /// </summary>
        /// <param name="request">Phone verification request</param>
        /// <returns>Verification result</returns>
        Task<AuthResultDto> VerifyPhoneAsync(VerifyPhoneDto request);

        /// <summary>
        /// Resend verification code
        /// </summary>
        /// <param name="request">Resend verification request</param>
        /// <returns>Resend result</returns>
        Task<AuthResultDto> ResendVerificationCodeAsync(ResendVerificationCodeDto request);

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="request">Password reset request</param>
        /// <returns>Reset result</returns>
        Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto request);

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Change password request</param>
        /// <returns>Change result</returns>
        Task<AuthResultDto> ChangePasswordAsync(ChangePasswordDto request);

        /// <summary>
        /// Refresh authentication token
        /// </summary>
        /// <param name="request">Token refresh request</param>
        /// <returns>Refresh result</returns>
        Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto request);

        /// <summary>
        /// Logout user
        /// </summary>
        /// <param name="request">Logout request</param>
        /// <returns>Logout result</returns>
        Task<AuthResultDto> LogoutAsync(LogoutDto request);
    }
}

