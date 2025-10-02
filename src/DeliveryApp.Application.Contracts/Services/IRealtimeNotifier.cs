using System;
using System.Threading.Tasks;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IRealtimeNotifier
    {
        Task NotifyUserApprovedAsync(Guid userId);
        Task NotifyOrderStatusChangedAsync(Guid orderId, string status);
    }
}
