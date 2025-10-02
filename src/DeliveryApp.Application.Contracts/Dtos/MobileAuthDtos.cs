using System;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class MobileAuthResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; }
        public string ErrorCode { get; set; }
        public bool RequiresVerification { get; set; }
        public string VerificationType { get; set; }
    }
} 
