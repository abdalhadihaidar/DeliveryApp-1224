using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Services
{
    public class SignalRNotifier : ApplicationService, IRealtimeNotifier
    {
        private readonly IHubContext<UserApprovalHub> _hubContext;
        private readonly ILogger<SignalRNotifier> _logger;

        public SignalRNotifier(
            IHubContext<UserApprovalHub> hubContext,
            ILogger<SignalRNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyUserApprovedAsync(Guid userId)
        {
            try
            {
                await _hubContext.Clients.User(userId.ToString()).SendAsync("UserApproved", new
                {
                    userId = userId,
                    message = "Your account has been approved by admin",
                    timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation($"SignalR notification sent for user approval: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SignalR notification for user approval: {userId}");
            }
        }

        public async Task NotifyOrderStatusChangedAsync(Guid orderId, string status)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("OrderStatusChanged", new
                {
                    orderId = orderId,
                    status = status,
                    timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation($"SignalR notification sent for order status change: {orderId} -> {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SignalR notification for order status: {orderId}");
            }
        }
    }
}
