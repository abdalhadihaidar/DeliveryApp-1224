using System.Threading.Tasks;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IEmailNotifier
    {
        Task SendAccountApprovedAsync(string email, string userName);
        Task SendWelcomeAsync(string email, string userName);
        Task SendOrderStatusAsync(string email, string orderNumber, string status);
    }
}
