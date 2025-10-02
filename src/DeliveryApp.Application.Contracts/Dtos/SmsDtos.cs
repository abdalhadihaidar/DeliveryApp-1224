using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class SendVerificationCodeDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string CountryCode { get; set; }
    }

    public class VerifyCodeDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        [StringLength(6, MinimumLength = 4)]
        public string Code { get; set; }
    }

    public class SmsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }
}

