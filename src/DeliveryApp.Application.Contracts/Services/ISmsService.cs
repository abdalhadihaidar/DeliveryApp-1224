using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Services
{
    [RemoteService]
    public interface ISmsService : IApplicationService
    {
        Task<bool> SendVerificationCodeAsync(string phoneNumber, string countryCode);
        Task<bool> VerifyCodeAsync(string phoneNumber, string code);
        Task<bool> SendPasswordResetCodeAsync(string phoneNumber);
        Task<bool> SendOrderNotificationAsync(string phoneNumber, string message);
    }
}

