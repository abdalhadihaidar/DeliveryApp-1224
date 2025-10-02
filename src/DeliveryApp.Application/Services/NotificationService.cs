using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using DeliveryApp.Hubs;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeliveryApp.Services
{
    public interface INotificationService
    {
        Task SendOrderStatusUpdateAsync(string orderId, string status, string message);
        Task SendOrderNotificationToRestaurantAsync(string restaurantId, string orderId, string message);
        Task SendDeliveryAssignmentAsync(string deliveryPersonId, string orderId, string message);
        Task SendLocationUpdateAsync(string orderId, double latitude, double longitude, string deliveryPersonId);
        Task SendGeneralNotificationAsync(string userId, string title, string message);
    }

    public class NotificationService : INotificationService, ITransientDependency
    {
        private readonly IHubContext<OrderTrackingHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<OrderTrackingHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendOrderStatusUpdateAsync(string orderId, string status, string message)
        {
            try
            {
                await _hubContext.Clients.Group($"Order_{orderId}")
                    .SendAsync("OrderStatusUpdate", new
                    {
                        OrderId = orderId,
                        Status = status,
                        Message = message,
                        Timestamp = System.DateTime.UtcNow
                    });

                _logger.LogInformation($"Order status update sent for order {orderId}: {status}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order status update for order {orderId}");
            }
        }

        public async Task SendOrderNotificationToRestaurantAsync(string restaurantId, string orderId, string message)
        {
            try
            {
                await _hubContext.Clients.Group($"Restaurant_{restaurantId}")
                    .SendAsync("NewOrderNotification", new
                    {
                        RestaurantId = restaurantId,
                        OrderId = orderId,
                        Message = message,
                        Timestamp = System.DateTime.UtcNow
                    });

                _logger.LogInformation($"New order notification sent to restaurant {restaurantId} for order {orderId}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order notification to restaurant {restaurantId}");
            }
        }

        public async Task SendDeliveryAssignmentAsync(string deliveryPersonId, string orderId, string message)
        {
            try
            {
                await _hubContext.Clients.Group($"Delivery_{deliveryPersonId}")
                    .SendAsync("DeliveryAssignment", new
                    {
                        DeliveryPersonId = deliveryPersonId,
                        OrderId = orderId,
                        Message = message,
                        Timestamp = System.DateTime.UtcNow
                    });

                _logger.LogInformation($"Delivery assignment sent to {deliveryPersonId} for order {orderId}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to send delivery assignment to {deliveryPersonId}");
            }
        }

        public async Task SendLocationUpdateAsync(string orderId, double latitude, double longitude, string deliveryPersonId)
        {
            try
            {
                await _hubContext.Clients.Group($"Order_{orderId}")
                    .SendAsync("DeliveryLocationUpdate", new
                    {
                        OrderId = orderId,
                        DeliveryPersonId = deliveryPersonId,
                        Latitude = latitude,
                        Longitude = longitude,
                        Timestamp = System.DateTime.UtcNow
                    });

                _logger.LogInformation($"Location update sent for order {orderId}: {latitude}, {longitude}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to send location update for order {orderId}");
            }
        }

        public async Task SendGeneralNotificationAsync(string userId, string title, string message)
        {
            try
            {
                await _hubContext.Clients.User(userId)
                    .SendAsync("GeneralNotification", new
                    {
                        Title = title,
                        Message = message,
                        Timestamp = System.DateTime.UtcNow
                    });

                _logger.LogInformation($"General notification sent to user {userId}: {title}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to send general notification to user {userId}");
            }
        }
    }
}

