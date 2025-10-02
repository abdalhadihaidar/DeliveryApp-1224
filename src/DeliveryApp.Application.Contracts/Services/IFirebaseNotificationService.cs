using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Firebase Cloud Messaging service interface
    /// </summary>
    public interface IFirebaseNotificationService
    {
        /// <summary>
        /// Send push notification to a single device
        /// </summary>
        Task<NotificationResult> SendNotificationAsync(string deviceToken, NotificationMessage message);

        /// <summary>
        /// Send push notification to multiple devices
        /// </summary>
        Task<List<NotificationResult>> SendNotificationToMultipleAsync(List<string> deviceTokens, NotificationMessage message);

        /// <summary>
        /// Send push notification to a topic
        /// </summary>
        Task<NotificationResult> SendNotificationToTopicAsync(string topic, NotificationMessage message);

        /// <summary>
        /// Subscribe user to a notification topic
        /// </summary>
        Task<bool> SubscribeToTopicAsync(string deviceToken, string topic);

        /// <summary>
        /// Unsubscribe user from a notification topic
        /// </summary>
        Task<bool> UnsubscribeFromTopicAsync(string deviceToken, string topic);

        /// <summary>
        /// Schedule a notification for later delivery
        /// </summary>
        Task<string> ScheduleNotificationAsync(ScheduledNotification notification);

        /// <summary>
        /// Cancel a scheduled notification
        /// </summary>
        Task<bool> CancelScheduledNotificationAsync(string notificationId);

        /// <summary>
        /// Get notification delivery status
        /// </summary>
        Task<NotificationResult> GetNotificationStatusAsync(string notificationId);
    }
}
