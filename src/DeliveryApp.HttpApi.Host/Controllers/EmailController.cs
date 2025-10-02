using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    /// <summary>
    /// Email service API controller for SendPulse integration
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : AbpControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Send email verification code
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Email sending result</returns>
        [HttpPost("send-verification")]
        [AllowAnonymous]
        public async Task<EmailSendResultDto> SendVerificationEmailAsync([FromBody] SendVerificationEmailDto request)
        {
            return await _emailService.SendVerificationEmailAsync(request);
        }

        /// <summary>
        /// Verify email verification code
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify-code")]
        [AllowAnonymous]
        public async Task<EmailVerificationResultDto> VerifyEmailCodeAsync([FromBody] VerifyEmailCodeDto request)
        {
            return await _emailService.VerifyEmailCodeAsync(request);
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <param name="request">Password reset email request</param>
        /// <returns>Email sending result</returns>
        [HttpPost("send-password-reset")]
        [AllowAnonymous]
        public async Task<EmailSendResultDto> SendPasswordResetEmailAsync([FromBody] SendPasswordResetEmailDto request)
        {
            return await _emailService.SendPasswordResetEmailAsync(request);
        }

        /// <summary>
        /// Send order notification email
        /// </summary>
        /// <param name="request">Order notification email request</param>
        /// <returns>Email sending result</returns>
        [HttpPost("send-order-notification")]
        [Authorize]
        public async Task<EmailSendResultDto> SendOrderNotificationEmailAsync([FromBody] SendOrderNotificationEmailDto request)
        {
            return await _emailService.SendOrderNotificationEmailAsync(request);
        }

        /// <summary>
        /// Resend verification email
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Email sending result</returns>
        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<EmailSendResultDto> ResendVerificationEmailAsync([FromBody] SendVerificationEmailDto request)
        {
            return await _emailService.SendVerificationEmailAsync(request);
        }
    }
}

