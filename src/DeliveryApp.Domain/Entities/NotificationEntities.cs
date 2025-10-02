using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeliveryApp.Domain.Entities
{
    /// <summary>
    /// Notification campaign entity
    /// </summary>
    public class NotificationCampaign : FullAuditedAggregateRoot<Guid>
    {
        public Guid RestaurantId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public CampaignStatus Status { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public DateTime? SentAt { get; set; }
        public string TargetingCriteria { get; set; } = string.Empty;
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Notification template entity
    /// </summary>
    public class NotificationTemplate : FullAuditedAggregateRoot<Guid>
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public string DefaultTargetingCriteria { get; set; } = string.Empty;
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Notification tracking entity for analytics
    /// </summary>
    public class NotificationTracking : FullAuditedAggregateRoot<Guid>
    {
        public Guid CampaignId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string DeviceToken { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClickedAt { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Notification status enumeration
    /// </summary>
    public enum NotificationStatus
    {
        Pending,
        Sent,
        Delivered,
        Failed,
        Opened,
        Clicked
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
    /// Notification type enumeration
    /// </summary>
    public enum NotificationType
    {
        OrderUpdate,
        General,
        PaymentConfirmation,
        DeliveryUpdate,
        PromotionalOffer,
        RestaurantUpdate,
        SecurityAlert,
        NewsUpdate,
        SurveyRequest,
        SystemMaintenance,
        Custom,
        Message
    }

    /// <summary>
    /// Notification priority enumeration
    /// </summary>
    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}
