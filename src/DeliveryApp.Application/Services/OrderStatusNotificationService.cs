using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Services;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Comprehensive order status notification service that coordinates both real-time and push notifications
    /// </summary>
    public class OrderStatusNotificationService : IOrderStatusNotificationService, ITransientDependency
    {
        private readonly INotificationService _signalRNotifier;
        private readonly IFirebaseNotificationService _firebaseNotifier;
        private readonly IOrderRepository _orderRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<OrderStatusNotificationService> _logger;

        public OrderStatusNotificationService(
            INotificationService signalRNotifier,
            IFirebaseNotificationService firebaseNotifier,
            IOrderRepository orderRepository,
            IRestaurantRepository restaurantRepository,
            IUserRepository userRepository,
            ILogger<OrderStatusNotificationService> logger)
        {
            _signalRNotifier = signalRNotifier;
            _firebaseNotifier = firebaseNotifier;
            _orderRepository = orderRepository;
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task NotifyOrderStatusChangeAsync(Guid orderId, OrderStatus previousStatus, OrderStatus newStatus, Guid? triggeredByUserId = null)
        {
            try
            {
                _logger.LogInformation($"Processing order status change notification for order {orderId}: {previousStatus} -> {newStatus}");

                // Get order details with all related entities
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found for status change notification");
                    return;
                }

                // Get restaurant details
                var restaurant = await _restaurantRepository.GetWithCategoryAsync(order.RestaurantId);

                // Send notifications to all relevant parties based on the new status
                await NotifyCustomerAsync(orderId, newStatus, order.UserId.ToString());
                await NotifyRestaurantOwnerAsync(orderId, newStatus, order.RestaurantId);
                await NotifyAdminDashboardAsync(orderId, newStatus);

                // Send delivery person notifications only if applicable
                if (order.DeliveryPersonId.HasValue)
                {
                    await NotifyDeliveryPersonAsync(orderId, newStatus, order.DeliveryPersonId);
                }

                // Special handling for specific status transitions
                await HandleSpecialStatusTransitions(order, previousStatus, newStatus);

                _logger.LogInformation($"Successfully sent all notifications for order {orderId} status change");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notifications for order {orderId} status change");
                // Don't rethrow - notification failures shouldn't break the order flow
            }
        }

        public async Task NotifyCustomerAsync(Guid orderId, OrderStatus status, string customerUserId)
        {
            try
            {
                var message = GetCustomerMessage(status);
                var title = GetCustomerTitle(status);

                // Send real-time notification via SignalR
                await _signalRNotifier.SendOrderStatusUpdateAsync(orderId.ToString(), status.ToString(), message);

                // Send push notification via Firebase
                var notificationMessage = new NotificationMessage
                {
                    Title = title,
                    Body = message,
                    Data = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "status", status.ToString() },
                        { "type", "order_status_update" },
                        { "targetScreen", "OrderDetails" }
                    }
                };

                // Get user's device tokens and send push notification
                var user = await _userRepository.GetAsync(Guid.Parse(customerUserId));
                if (!string.IsNullOrEmpty(user.DeviceToken))
                {
                    await _firebaseNotifier.SendNotificationAsync(user.DeviceToken, notificationMessage);
                }

                _logger.LogInformation($"Customer notification sent for order {orderId}, status: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send customer notification for order {orderId}");
            }
        }

        public async Task NotifyRestaurantOwnerAsync(Guid orderId, OrderStatus status, Guid restaurantId)
        {
            try
            {
                var message = GetRestaurantOwnerMessage(status);
                var title = GetRestaurantOwnerTitle(status);

                // Send real-time notification via SignalR to restaurant group
                await _signalRNotifier.SendOrderNotificationToRestaurantAsync(restaurantId.ToString(), orderId.ToString(), message);

                // Get restaurant owner and send push notification
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                var owner = await _userRepository.GetAsync(restaurant.OwnerId);

                var notificationMessage = new NotificationMessage
                {
                    Title = title,
                    Body = message,
                    Data = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "restaurantId", restaurantId.ToString() },
                        { "status", status.ToString() },
                        { "type", "restaurant_order_update" },
                        { "targetScreen", "RestaurantOrders" }
                    }
                };

                if (!string.IsNullOrEmpty(owner.DeviceToken))
                {
                    await _firebaseNotifier.SendNotificationAsync(owner.DeviceToken, notificationMessage);
                }

                _logger.LogInformation($"Restaurant owner notification sent for order {orderId}, status: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send restaurant owner notification for order {orderId}");
            }
        }

        public async Task NotifyDeliveryPersonAsync(Guid orderId, OrderStatus status, Guid? deliveryPersonId)
        {
            if (!deliveryPersonId.HasValue) return;

            try
            {
                var message = GetDeliveryPersonMessage(status);
                var title = GetDeliveryPersonTitle(status);

                // Send real-time notification via SignalR to delivery person group
                await _signalRNotifier.SendDeliveryAssignmentAsync(deliveryPersonId.ToString(), orderId.ToString(), message);

                // Get delivery person and send push notification
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId.Value);

                var notificationMessage = new NotificationMessage
                {
                    Title = title,
                    Body = message,
                    Data = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "status", status.ToString() },
                        { "type", "delivery_status_update" },
                        { "targetScreen", "DeliveryOrders" }
                    }
                };

                if (!string.IsNullOrEmpty(deliveryPerson.DeviceToken))
                {
                    await _firebaseNotifier.SendNotificationAsync(deliveryPerson.DeviceToken, notificationMessage);
                }

                _logger.LogInformation($"Delivery person notification sent for order {orderId}, status: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send delivery person notification for order {orderId}");
            }
        }

        public async Task NotifyAdminDashboardAsync(Guid orderId, OrderStatus status)
        {
            try
            {
                var message = $"Order {orderId} status updated to {status}";

                // Send real-time notification to admin dashboard via SignalR
                await _signalRNotifier.SendGeneralNotificationAsync("admin", "Order Status Update", message);

                _logger.LogInformation($"Admin dashboard notification sent for order {orderId}, status: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send admin dashboard notification for order {orderId}");
            }
        }

        public async Task NotifyDeliveryAssignmentAsync(Guid orderId, Guid deliveryPersonId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                var restaurant = await _restaurantRepository.GetAsync(order.RestaurantId);

                var message = $"New delivery assignment: Order #{orderId} from {restaurant.Name}";
                var title = "New Delivery Assignment";

                // Send real-time notification
                await _signalRNotifier.SendDeliveryAssignmentAsync(deliveryPersonId.ToString(), orderId.ToString(), message);

                // Send push notification
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                var notificationMessage = new NotificationMessage
                {
                    Title = title,
                    Body = message,
                    Data = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "orderId", orderId.ToString() },
                        { "restaurantId", order.RestaurantId.ToString() },
                        { "type", "delivery_assignment" },
                        { "targetScreen", "DeliveryAssignment" }
                    }
                };

                if (!string.IsNullOrEmpty(deliveryPerson.DeviceToken))
                {
                    await _firebaseNotifier.SendNotificationAsync(deliveryPerson.DeviceToken, notificationMessage);
                }

                _logger.LogInformation($"Delivery assignment notification sent for order {orderId} to delivery person {deliveryPersonId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send delivery assignment notification for order {orderId}");
            }
        }

        public async Task NotifyLocationUpdateAsync(Guid orderId, double latitude, double longitude, Guid deliveryPersonId)
        {
            try
            {
                // Send real-time location update via SignalR
                await _signalRNotifier.SendLocationUpdateAsync(orderId.ToString(), latitude, longitude, deliveryPersonId.ToString());

                _logger.LogInformation($"Location update sent for order {orderId}: {latitude}, {longitude}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send location update for order {orderId}");
            }
        }

        private async Task HandleSpecialStatusTransitions(Domain.Entities.Order order, OrderStatus previousStatus, OrderStatus newStatus)
        {
            // Handle new order notification to restaurant
            if (previousStatus == OrderStatus.Pending && newStatus == OrderStatus.Pending && order.PaymentStatus == PaymentStatus.Paid)
            {
                // This is a new paid order - send special notification to restaurant
                var restaurant = await _restaurantRepository.GetAsync(order.RestaurantId);
                var specialMessage = $"New order received! Order #{order.Id} - Total: ${order.TotalAmount:F2}";
                await _signalRNotifier.SendOrderNotificationToRestaurantAsync(order.RestaurantId.ToString(), order.Id.ToString(), specialMessage);
            }

            // Handle delivery assignment when order becomes ready for delivery
            if (newStatus == OrderStatus.ReadyForDelivery)
            {
                // Notify all available delivery persons about new order available for pickup
                // This would be implemented as a broadcast to all delivery persons in the area
                var message = $"New order available for delivery from {order.Restaurant?.Name}";
                // Note: This would require additional implementation to get delivery persons in the area
            }
        }

        #region Message Generators

        private string GetCustomerMessage(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Your order has been placed and is waiting for restaurant confirmation.",
                OrderStatus.Preparing => "Great news! The restaurant has accepted your order and it's now being prepared.",
                OrderStatus.ReadyForDelivery => "Your order is ready and we're finding a delivery person for you.",
                OrderStatus.WaitingCourier => "Your order is prepared and waiting for courier assignment.",
                OrderStatus.Delivering => "Your order is on its way! Track your delivery in real-time.",
                OrderStatus.Delivered => "Your order has been delivered successfully. Enjoy your meal!",
                OrderStatus.Cancelled => "Your order has been cancelled. If you have any questions, please contact support.",
                _ => "Your order status has been updated."
            };
        }

        private string GetCustomerTitle(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Order Placed",
                OrderStatus.Preparing => "Order Accepted",
                OrderStatus.ReadyForDelivery => "Order Ready",
                OrderStatus.WaitingCourier => "Finding Delivery Person",
                OrderStatus.Delivering => "Order On The Way",
                OrderStatus.Delivered => "Order Delivered",
                OrderStatus.Cancelled => "Order Cancelled",
                _ => "Order Update"
            };
        }

        private string GetRestaurantOwnerMessage(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "New order received! Please review and accept/reject the order.",
                OrderStatus.Preparing => "Order accepted and marked as being prepared.",
                OrderStatus.ReadyForDelivery => "Order marked as ready for delivery. Looking for delivery person.",
                OrderStatus.WaitingCourier => "Order is waiting for courier assignment.",
                OrderStatus.Delivering => "Order has been picked up by delivery person.",
                OrderStatus.Delivered => "Order has been successfully delivered to customer.",
                OrderStatus.Cancelled => "Order has been cancelled.",
                _ => "Order status has been updated."
            };
        }

        private string GetRestaurantOwnerTitle(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "New Order",
                OrderStatus.Preparing => "Order Accepted",
                OrderStatus.ReadyForDelivery => "Order Ready",
                OrderStatus.WaitingCourier => "Awaiting Pickup",
                OrderStatus.Delivering => "Order Picked Up",
                OrderStatus.Delivered => "Order Completed",
                OrderStatus.Cancelled => "Order Cancelled",
                _ => "Order Update"
            };
        }

        private string GetDeliveryPersonMessage(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.ReadyForDelivery => "New delivery opportunity available for pickup.",
                OrderStatus.WaitingCourier => "Order is ready for pickup at the restaurant.",
                OrderStatus.Delivering => "Order assigned to you. Please proceed to pickup location.",
                OrderStatus.Delivered => "Order delivery completed successfully.",
                OrderStatus.Cancelled => "Assigned order has been cancelled.",
                _ => "Order status has been updated."
            };
        }

        private string GetDeliveryPersonTitle(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.ReadyForDelivery => "Delivery Available",
                OrderStatus.WaitingCourier => "Ready for Pickup",
                OrderStatus.Delivering => "Delivery Assignment",
                OrderStatus.Delivered => "Delivery Completed",
                OrderStatus.Cancelled => "Order Cancelled",
                _ => "Delivery Update"
            };
        }

        #endregion
    }
}
