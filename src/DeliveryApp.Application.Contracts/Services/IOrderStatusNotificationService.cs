using System;
using System.Threading.Tasks;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Service for sending comprehensive notifications when order status changes
    /// Handles both real-time (SignalR) and push notifications (Firebase) to all relevant parties
    /// </summary>
    public interface IOrderStatusNotificationService
    {
        /// <summary>
        /// Send notifications to all relevant parties when order status changes
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="previousStatus">Previous order status</param>
        /// <param name="newStatus">New order status</param>
        /// <param name="triggeredByUserId">User who triggered the status change</param>
        Task NotifyOrderStatusChangeAsync(Guid orderId, OrderStatus previousStatus, OrderStatus newStatus, Guid? triggeredByUserId = null);

        /// <summary>
        /// Send notification to customer about order status change
        /// </summary>
        Task NotifyCustomerAsync(Guid orderId, OrderStatus status, string customerUserId);

        /// <summary>
        /// Send notification to restaurant owner about order status change
        /// </summary>
        Task NotifyRestaurantOwnerAsync(Guid orderId, OrderStatus status, Guid restaurantId);

        /// <summary>
        /// Send notification to delivery person about order status change
        /// </summary>
        Task NotifyDeliveryPersonAsync(Guid orderId, OrderStatus status, Guid? deliveryPersonId);

        /// <summary>
        /// Send notification to admin dashboard about order status change
        /// </summary>
        Task NotifyAdminDashboardAsync(Guid orderId, OrderStatus status);

        /// <summary>
        /// Send assignment notification to delivery person when order is ready for delivery
        /// </summary>
        Task NotifyDeliveryAssignmentAsync(Guid orderId, Guid deliveryPersonId);

        /// <summary>
        /// Broadcast location update during delivery
        /// </summary>
        Task NotifyLocationUpdateAsync(Guid orderId, double latitude, double longitude, Guid deliveryPersonId);
    }
}
