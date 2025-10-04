using System;
using System.Threading.Tasks;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IRealtimeNotifier
    {
        Task NotifyUserApprovedAsync(Guid userId);
        Task NotifyUserRejectedAsync(Guid userId, string reason);
        Task NotifyOrderStatusChangedAsync(Guid orderId, string status);
    }
}
