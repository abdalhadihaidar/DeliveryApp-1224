using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IMobileAuthService
    {
        Task<MobileAuthResultDto> RegisterWithEmailAsync(RegisterWithEmailDto request);
        Task<MobileAuthResultDto> LoginWithEmailAsync(LoginWithEmailDto request);
        Task<MobileAuthResultDto> RefreshTokenAsync(RefreshTokenDto request);
        Task<MobileAuthResultDto> LogoutAsync(LogoutDto request);
        Task<UserInfoDto> GetUserInfoAsync(string userId);
    }
} 
