using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Service for restaurant owners to send targeted notifications to their previous customers
    /// </summary>
    public interface IRestaurantNotificationService : IApplicationService
    {
        /// <summary>
        /// Send targeted notification to restaurant's previous customers
        /// </summary>
        Task<RestaurantNotificationCampaignResult> SendTargetedNotificationAsync(RestaurantTargetedNotificationRequest request);

        /// <summary>
        /// Get restaurant's notification campaign history
        /// </summary>
        Task<List<RestaurantNotificationCampaignSummary>> GetRestaurantCampaignsAsync(Guid restaurantId, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get specific campaign details
        /// </summary>
        Task<RestaurantNotificationCampaignResult> GetCampaignDetailsAsync(string campaignId);

        /// <summary>
        /// Get restaurant notification analytics
        /// </summary>
        Task<RestaurantNotificationAnalytics> GetRestaurantAnalyticsAsync(Guid restaurantId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get customer count for targeting criteria (preview)
        /// </summary>
        Task<int> GetTargetedCustomerCountAsync(Guid restaurantId, CustomerTargetingCriteria criteria);

        /// <summary>
        /// Get customer segments for restaurant
        /// </summary>
        Task<Dictionary<CustomerSegment, int>> GetCustomerSegmentsAsync(Guid restaurantId);

        /// <summary>
        /// Create notification template for restaurant
        /// </summary>
        Task<RestaurantNotificationTemplate> CreateTemplateAsync(CreateRestaurantNotificationTemplateDto request);

        /// <summary>
        /// Get restaurant notification templates
        /// </summary>
        Task<List<RestaurantNotificationTemplate>> GetRestaurantTemplatesAsync(Guid restaurantId);

        /// <summary>
        /// Update notification template
        /// </summary>
        Task<RestaurantNotificationTemplate> UpdateTemplateAsync(string templateId, CreateRestaurantNotificationTemplateDto request);

        /// <summary>
        /// Delete notification template
        /// </summary>
        Task<bool> DeleteTemplateAsync(string templateId);

        /// <summary>
        /// Get customer notification preferences for restaurant
        /// </summary>
        Task<CustomerRestaurantNotificationPreferences> GetCustomerPreferencesAsync(Guid customerId, Guid restaurantId);

        /// <summary>
        /// Update customer notification preferences for restaurant
        /// </summary>
        Task<CustomerRestaurantNotificationPreferences> UpdateCustomerPreferencesAsync(Guid customerId, UpdateCustomerRestaurantNotificationPreferencesDto request);

        /// <summary>
        /// Cancel scheduled campaign
        /// </summary>
        Task<bool> CancelCampaignAsync(string campaignId);

        /// <summary>
        /// Resend failed notifications for a campaign
        /// </summary>
        Task<RestaurantNotificationCampaignResult> ResendFailedNotificationsAsync(string campaignId);

        /// <summary>
        /// Get campaign performance metrics
        /// </summary>
        Task<Dictionary<string, object>> GetCampaignMetricsAsync(string campaignId);

        /// <summary>
        /// Test notification with sample data
        /// </summary>
        Task<NotificationResult> TestNotificationAsync(Guid restaurantId, NotificationMessage message, List<string> testDeviceTokens);

        /// <summary>
        /// Get restaurant notification settings
        /// </summary>
        Task<RestaurantNotificationSettings> GetRestaurantNotificationSettingsAsync(Guid restaurantId);

        /// <summary>
        /// Update restaurant notification settings
        /// </summary>
        Task<RestaurantNotificationSettings> UpdateRestaurantNotificationSettingsAsync(Guid restaurantId, RestaurantNotificationSettings settings);
    }

    /// <summary>
    /// Restaurant notification settings
    /// </summary>
    public class RestaurantNotificationSettings
    {
        public Guid RestaurantId { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
        public int MaxNotificationsPerDay { get; set; } = 5;
        public int MaxNotificationsPerWeek { get; set; } = 20;
        public bool RequireApproval { get; set; } = false;
        public List<Guid> ApproverUserIds { get; set; } = new List<Guid>();
        public bool AllowPromotionalOffers { get; set; } = true;
        public bool AllowNewMenuItems { get; set; } = true;
        public bool AllowSpecialEvents { get; set; } = true;
        public bool AllowDiscountOffers { get; set; } = true;
        public bool AllowOrderReminders { get; set; } = true;
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public List<DayOfWeek> QuietDays { get; set; } = new List<DayOfWeek>();
        public DateTime UpdatedAt { get; set; }
    }
}
