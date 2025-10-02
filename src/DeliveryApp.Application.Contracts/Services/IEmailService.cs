using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Email service interface for SendPulse integration
    /// </summary>
    [RemoteService]
    public interface IEmailService : IApplicationService
    {
        /// <summary>
        /// Send verification email to user
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Email sending result</returns>
        Task<EmailSendResultDto> SendVerificationEmailAsync(SendVerificationEmailDto request);

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <param name="request">Password reset email request</param>
        /// <returns>Email sending result</returns>
        Task<EmailSendResultDto> SendPasswordResetEmailAsync(SendPasswordResetEmailDto request);

        /// <summary>
        /// Send order notification email
        /// </summary>
        /// <param name="request">Order notification email request</param>
        /// <returns>Email sending result</returns>
        Task<EmailSendResultDto> SendOrderNotificationEmailAsync(SendOrderNotificationEmailDto request);

        /// <summary>
        /// Verify email verification code
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Verification result</returns>
        Task<EmailVerificationResultDto> VerifyEmailCodeAsync(VerifyEmailCodeDto request);

        /// <summary>
        /// Send simple email notification
        /// </summary>
        /// <param name="email">Recipient email</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <returns>Email sending result</returns>
        Task<bool> SendEmailAsync(string email, string subject, string body);
    }
}

