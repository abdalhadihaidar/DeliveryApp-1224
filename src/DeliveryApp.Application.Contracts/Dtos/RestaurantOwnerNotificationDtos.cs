using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// DTO for sending notifications to restaurant customers
    /// </summary>
    public class SendRestaurantNotificationDto
    {
        [Required]
        public Guid RestaurantId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; } = NotificationType.General;
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public CustomerTargetingCriteria? TargetingCriteria { get; set; }
        
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
        
        public DateTime? ScheduledTime { get; set; }
    }

    /// <summary>
    /// Criteria for targeting specific customers
    /// </summary>
    public class CustomerTargetingCriteria
    {
        public List<Guid>? SpecificCustomerIds { get; set; }
        
        public CustomerSegment? Segment { get; set; }
        
        public DateTime? LastOrderAfter { get; set; }
        
        public DateTime? LastOrderBefore { get; set; }
        
        public decimal? MinOrderValue { get; set; }
        
        public decimal? MaxOrderValue { get; set; }
        
        public int? MinOrderCount { get; set; }
        
        public int? MaxOrderCount { get; set; }
        
        public List<string>? PreferredCategories { get; set; }
        
        public bool? HasFavoritedRestaurant { get; set; }
        
        public string? City { get; set; }
        
        public string? PreferredLanguage { get; set; }
    }

    /// <summary>
    /// Customer segment types for targeting
    /// </summary>
    public enum CustomerSegment
    {
        All,
        VIP,
        Active,
        Regular,
        Occasional,
        Inactive,
        NewCustomers,
        ReturningCustomers,
        HighValue,
        FrequentBuyers
    }

    /// <summary>
    /// DTO for customer information in restaurant context
    /// </summary>
    public class RestaurantCustomerDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string CustomerSegment { get; set; } = string.Empty;
        public bool IsFavorited { get; set; }
        public List<string> PreferredCategories { get; set; } = new List<string>();
        public string City { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "en";
        public string DeviceToken { get; set; } = string.Empty;
        public bool HasDeviceToken { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }

    /// <summary>
    /// DTO for notification campaign
    /// </summary>
    public class NotificationCampaignDto : EntityDto<Guid>
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public CustomerTargetingCriteria? TargetingCriteria { get; set; }
        public CampaignStatus Status { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public DateTime? SentAt { get; set; }
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Campaign status enumeration
    /// </summary>
    public enum CampaignStatus
    {
        Draft,
        Scheduled,
        Sending,
        Sent,
        Failed,
        Cancelled
    }

    /// <summary>
    /// DTO for notification analytics
    /// </summary>
    public class NotificationAnalyticsDto
    {
        public Guid CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public double DeliveryRate { get; set; }
        public int TotalOpened { get; set; }
        public double OpenRate { get; set; }
        public int TotalClicked { get; set; }
        public double ClickRate { get; set; }
        public DateTime SentAt { get; set; }
        public List<NotificationSegmentAnalytics> SegmentAnalytics { get; set; } = new List<NotificationSegmentAnalytics>();
    }

    /// <summary>
    /// Analytics for specific customer segments
    /// </summary>
    public class NotificationSegmentAnalytics
    {
        public string SegmentName { get; set; } = string.Empty;
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int OpenedCount { get; set; }
        public int ClickedCount { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    /// <summary>
    /// DTO for getting previous customers of a restaurant
    /// </summary>
    public class GetRestaurantCustomersDto : PagedAndSortedResultRequestDto
    {
        public Guid RestaurantId { get; set; }
        public CustomerSegment? Segment { get; set; }
        public DateTime? LastOrderAfter { get; set; }
        public DateTime? LastOrderBefore { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxOrderValue { get; set; }
        public int? MinOrderCount { get; set; }
        public int? MaxOrderCount { get; set; }
        public string? SearchTerm { get; set; }
        public bool? HasDeviceToken { get; set; }
        public bool? EmailNotificationsEnabled { get; set; }
        public bool? PushNotificationsEnabled { get; set; }
    }

    /// <summary>
    /// DTO for notification template
    /// </summary>
    public class NotificationTemplateDto : EntityDto<Guid>
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public CustomerTargetingCriteria? DefaultTargetingCriteria { get; set; }
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating notification template
    /// </summary>
    public class CreateNotificationTemplateDto
    {
        [Required]
        public Guid RestaurantId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; } = NotificationType.General;
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public CustomerTargetingCriteria? DefaultTargetingCriteria { get; set; }
        
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// DTO for updating notification template
    /// </summary>
    public class UpdateNotificationTemplateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(100)]
        public string? Title { get; set; }
        
        [StringLength(500)]
        public string? Message { get; set; }
        
        public NotificationType? Type { get; set; }
        
        public NotificationPriority? Priority { get; set; }
        
        public CustomerTargetingCriteria? DefaultTargetingCriteria { get; set; }
        
        public Dictionary<string, string>? CustomData { get; set; }
        
        public bool? IsActive { get; set; }
    }
}
