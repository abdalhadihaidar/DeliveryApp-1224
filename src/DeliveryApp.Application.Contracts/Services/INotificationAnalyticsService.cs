using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Notification analytics service interface
    /// </summary>
    public interface INotificationAnalyticsService
    {
        /// <summary>
        /// Track notification sent
        /// </summary>
        Task TrackNotificationSentAsync(NotificationTrackingData trackingData);

        /// <summary>
        /// Track notification opened
        /// </summary>
        Task TrackNotificationOpenedAsync(string notificationId, string userId, string deviceToken);

        /// <summary>
        /// Track notification clicked
        /// </summary>
        Task TrackNotificationClickedAsync(string notificationId, string userId, string deviceToken, string actionUrl);

        /// <summary>
        /// Get notification statistics
        /// </summary>
        Task<NotificationStatistics> GetNotificationStatisticsAsync(string notificationId);

        /// <summary>
        /// Get notification analytics
        /// </summary>
        Task<NotificationAnalytics> GetNotificationAnalyticsAsync(NotificationAnalyticsFilter filter);

        /// <summary>
        /// Get user engagement metrics
        /// </summary>
        Task<UserEngagementMetrics> GetUserEngagementMetricsAsync(string userId);

        /// <summary>
        /// Get campaign performance
        /// </summary>
        Task<CampaignPerformance> GetCampaignPerformanceAsync(string campaignId);

        /// <summary>
        /// Generate notification report
        /// </summary>
        Task<NotificationReport> GenerateNotificationReportAsync(NotificationReportRequest request);

        /// <summary>
        /// Get template statistics
        /// </summary>
        Task<TemplateStatistics> GetTemplateStatisticsAsync(string templateId);

        /// <summary>
        /// Update notification preferences
        /// </summary>
        Task UpdateNotificationPreferencesAsync(string userId, NotificationPreferences preferences);

        /// <summary>
        /// Get notification preferences
        /// </summary>
        Task<NotificationPreferences> GetNotificationPreferencesAsync(string userId);
    }
}
