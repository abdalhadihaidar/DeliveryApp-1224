using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Email service DTOs for SendPulse integration
    /// </summary>
    public class SendVerificationEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        public string Language { get; set; } = "en";
    }

    public class SendPasswordResetEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        public string Language { get; set; } = "en";
    }

    public class SendOrderNotificationEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string OrderId { get; set; }

        [Required]
        public string OrderStatus { get; set; }

        public string Language { get; set; } = "en";
    }

    public class VerifyEmailCodeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; }
    }

    public class EmailSendResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string MessageId { get; set; }
        public string ErrorCode { get; set; }
    }

    public class EmailVerificationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsExpired { get; set; }
        public bool IsAlreadyUsed { get; set; }
    }

    /// <summary>
    /// SendPulse configuration settings
    /// </summary>
    public class SendPulseSettings
    {
        public string ApiUserId { get; set; }
        public string ApiSecret { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string VerificationTemplateId { get; set; }
        public string PasswordResetTemplateId { get; set; }
        public string OrderNotificationTemplateId { get; set; }
        public int VerificationCodeExpiryMinutes { get; set; } = 15;
    }
}

