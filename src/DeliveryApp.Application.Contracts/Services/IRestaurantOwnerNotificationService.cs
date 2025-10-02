using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;


namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Service for restaurant owners to send targeted notifications to their customers
    /// </summary>
    public interface IRestaurantOwnerNotificationService
    {
        /// <summary>
        /// Send notification to restaurant's previous customers
        /// </summary>
        Task<NotificationCampaignDto> SendNotificationToCustomersAsync(SendRestaurantNotificationDto dto, string restaurantOwnerId);

        /// <summary>
        /// Get previous customers of a restaurant with filtering options
        /// </summary>
        Task<PagedResultDto<RestaurantCustomerDto>> GetRestaurantCustomersAsync(GetRestaurantCustomersDto dto, string restaurantOwnerId);

        /// <summary>
        /// Get customer segments for a restaurant
        /// </summary>
        Task<List<CustomerSegmentDto>> GetCustomerSegmentsAsync(Guid restaurantId, string restaurantOwnerId);

        /// <summary>
        /// Get notification campaigns for a restaurant
        /// </summary>
        Task<PagedResultDto<NotificationCampaignDto>> GetNotificationCampaignsAsync(Guid restaurantId, PagedAndSortedResultRequestDto input, string restaurantOwnerId);

        /// <summary>
        /// Get notification campaign details
        /// </summary>
        Task<NotificationCampaignDto> GetNotificationCampaignAsync(Guid campaignId, string restaurantOwnerId);

        /// <summary>
        /// Get notification analytics for a campaign
        /// </summary>
        Task<NotificationAnalyticsDto> GetNotificationAnalyticsAsync(Guid campaignId, string restaurantOwnerId);

        /// <summary>
        /// Cancel a scheduled notification campaign
        /// </summary>
        Task<bool> CancelNotificationCampaignAsync(Guid campaignId, string restaurantOwnerId);

        /// <summary>
        /// Create notification template
        /// </summary>
        Task<NotificationTemplateDto> CreateNotificationTemplateAsync(CreateNotificationTemplateDto dto, string restaurantOwnerId);

        /// <summary>
        /// Update notification template
        /// </summary>
        Task<NotificationTemplateDto> UpdateNotificationTemplateAsync(Guid templateId, UpdateNotificationTemplateDto dto, string restaurantOwnerId);

        /// <summary>
        /// Get notification templates for a restaurant
        /// </summary>
        Task<List<NotificationTemplateDto>> GetNotificationTemplatesAsync(Guid restaurantId, string restaurantOwnerId);

        /// <summary>
        /// Delete notification template
        /// </summary>
        Task<bool> DeleteNotificationTemplateAsync(Guid templateId, string restaurantOwnerId);

        /// <summary>
        /// Send notification using template
        /// </summary>
        Task<NotificationCampaignDto> SendNotificationFromTemplateAsync(Guid templateId, CustomerTargetingCriteria? customTargeting, string restaurantOwnerId);

        /// <summary>
        /// Get notification statistics for restaurant dashboard
        /// </summary>
        Task<RestaurantNotificationStatsDto> GetNotificationStatisticsAsync(Guid restaurantId, DateTime? startDate, DateTime? endDate, string restaurantOwnerId);
    }

    /// <summary>
    /// DTO for customer segment information
    /// </summary>
    public class CustomerSegmentDto
    {
        public CustomerSegment Segment { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime LastActivity { get; set; }
    }

    /// <summary>
    /// DTO for restaurant notification statistics
    /// </summary>
    public class RestaurantNotificationStatsDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public int TotalCampaigns { get; set; }
        public int TotalNotificationsSent { get; set; }
        public int TotalNotificationsDelivered { get; set; }
        public double AverageDeliveryRate { get; set; }
        public int TotalNotificationsOpened { get; set; }
        public double AverageOpenRate { get; set; }
        public int TotalNotificationsClicked { get; set; }
        public double AverageClickRate { get; set; }
        public int ActiveCustomers { get; set; }
        public int CustomersWithDeviceTokens { get; set; }
        public int CustomersWithEmailEnabled { get; set; }
        public int CustomersWithPushEnabled { get; set; }
        public List<NotificationCampaignDto> RecentCampaigns { get; set; } = new List<NotificationCampaignDto>();
        public List<CustomerSegmentDto> CustomerSegments { get; set; } = new List<CustomerSegmentDto>();
    }
}
