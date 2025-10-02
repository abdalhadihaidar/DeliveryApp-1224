using DeliveryApp.Application.Contracts.Dtos;
using System;
using System.Threading.Tasks;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Secure authentication service interface with transaction management
    /// </summary>
    public interface ISecureAuthService
    {
        /// <summary>
        /// Secure user registration with transaction management
        /// </summary>
        Task<AuthResultDto> RegisterWithEmailSecureAsync(RegisterWithEmailDto request);

        /// <summary>
        /// Secure user profile update with validation
        /// </summary>
        Task<AuthResultDto> UpdateUserProfileSecureAsync(Guid userId, SecureUpdateUserProfileDto request);

        /// <summary>
        /// Secure password change with transaction management
        /// </summary>
        Task<AuthResultDto> ChangePasswordSecureAsync(Guid userId, SecureChangePasswordDto request);

        /// <summary>
        /// Secure account deletion with cleanup
        /// </summary>
        Task<AuthResultDto> DeleteAccountSecureAsync(Guid userId, string password);
    }
}
