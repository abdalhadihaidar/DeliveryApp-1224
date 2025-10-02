using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using DeliveryApp.Services;
using Volo.Abp.AspNetCore.Mvc;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Localization;

namespace DeliveryApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsController : AbpControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;

        public SmsController(ISmsService smsService, IStringLocalizer<DeliveryAppResource> localizer)
        {
            _smsService = smsService;
            _localizer = localizer;
        }

        [HttpPost("send-verification")]
        public async Task<SmsResponseDto> SendVerificationCode([FromBody] SendVerificationCodeDto input)
        {
            var success = await _smsService.SendVerificationCodeAsync(input.PhoneNumber, input.CountryCode);
            
            return new SmsResponseDto
            {
                Success = success,
                Message = success ? _localizer["Sms:VerificationCodeSent"] : _localizer["Sms:VerificationCodeFailed"],
                ErrorCode = success ? null : "SMS_SEND_FAILED"
            };
        }

        [HttpPost("verify-code")]
        public async Task<SmsResponseDto> VerifyCode([FromBody] VerifyCodeDto input)
        {
            var success = await _smsService.VerifyCodeAsync(input.PhoneNumber, input.Code);
            
            return new SmsResponseDto
            {
                Success = success,
                Message = success ? _localizer["Sms:PhoneVerified"] : _localizer["Sms:InvalidCode"],
                ErrorCode = success ? null : "VERIFICATION_FAILED"
            };
        }

        [HttpPost("send-password-reset")]
        public async Task<SmsResponseDto> SendPasswordResetCode([FromBody] string phoneNumber)
        {
            var success = await _smsService.SendPasswordResetCodeAsync(phoneNumber);
            
            return new SmsResponseDto
            {
                Success = success,
                Message = success ? _localizer["Sms:PasswordResetSent"] : _localizer["Sms:PasswordResetFailed"],
                ErrorCode = success ? null : "PASSWORD_RESET_FAILED"
            };
        }
    }
}

