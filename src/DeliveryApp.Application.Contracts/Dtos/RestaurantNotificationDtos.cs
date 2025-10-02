using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Request to send targeted notification to restaurant's previous customers
    /// </summary>
    public class RestaurantTargetedNotificationRequest
    {
        [Required]
        public Guid RestaurantId { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Message { get; set; }
        
        public string ImageUrl { get; set; }
        
        public string ActionUrl { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public NotificationType Type { get; set; } = NotificationType.PromotionalOffer;
        
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
        
        // Targeting criteria
        public CustomerTargetingCriteria TargetingCriteria { get; set; } = new CustomerTargetingCriteria();
        
        // Scheduling
        public DateTime? ScheduledTime { get; set; }
        
        public bool SendImmediately { get; set; } = true;
    }


    /// <summary>
    /// Result of targeted notification campaign
    /// </summary>
    public class RestaurantNotificationCampaignResult
    {
        public string CampaignId { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public DateTime? SentAt { get; set; }
        
        // Targeting results
        public int TotalTargetedCustomers { get; set; }
        public int CustomersWithValidTokens { get; set; }
        public int CustomersExcluded { get; set; }
        
        // Delivery results
        public int TotalSent { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedTokens { get; set; } = new List<string>();
        
        // Performance metrics
        public double DeliveryRate { get; set; }
        public double EstimatedOpenRate { get; set; }
        public double EstimatedClickRate { get; set; }
        
        // Campaign details
        public string Title { get; set; }
        public NotificationType Type { get; set; }
        public CustomerTargetingCriteria TargetingCriteria { get; set; }
    }


    /// <summary>
    /// Restaurant notification campaign summary
    /// </summary>
    public class RestaurantNotificationCampaignSummary
    {
        public string CampaignId { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string Title { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public CampaignStatus Status { get; set; }
        
        // Performance metrics
        public int TotalTargeted { get; set; }
        public int TotalSent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        
        // Customer engagement
        public int NewCustomersReached { get; set; }
        public int ReturningCustomersReached { get; set; }
        public int HighValueCustomersReached { get; set; }
    }


    /// <summary>
    /// Restaurant notification analytics
    /// </summary>
    public class RestaurantNotificationAnalytics
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Overall metrics
        public int TotalCampaigns { get; set; }
        public int TotalNotificationsSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        
        // Performance rates
        public double OverallDeliveryRate { get; set; }
        public double OverallOpenRate { get; set; }
        public double OverallClickRate { get; set; }
        
        // Customer engagement
        public int UniqueCustomersReached { get; set; }
        public int NewCustomersAcquired { get; set; }
        public int ReturningCustomersRetained { get; set; }
        
        // Campaign breakdown
        public Dictionary<NotificationType, CampaignTypeMetrics> TypeBreakdown { get; set; } = new Dictionary<NotificationType, CampaignTypeMetrics>();
        public List<DailyCampaignMetrics> DailyMetrics { get; set; } = new List<DailyCampaignMetrics>();
        
        // Top performing campaigns
        public List<RestaurantNotificationCampaignSummary> TopCampaigns { get; set; } = new List<RestaurantNotificationCampaignSummary>();
    }

    /// <summary>
    /// Campaign type metrics
    /// </summary>
    public class CampaignTypeMetrics
    {
        public NotificationType Type { get; set; }
        public int CampaignCount { get; set; }
        public int NotificationsSent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        public double AverageEngagement { get; set; }
    }

    /// <summary>
    /// Daily campaign metrics
    /// </summary>
    public class DailyCampaignMetrics
    {
        public DateTime Date { get; set; }
        public int CampaignsLaunched { get; set; }
        public int NotificationsSent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    /// <summary>
    /// Customer notification preferences for restaurant
    /// </summary>
    public class CustomerRestaurantNotificationPreferences
    {
        public Guid CustomerId { get; set; }
        public Guid RestaurantId { get; set; }
        public bool AllowPromotionalOffers { get; set; } = true;
        public bool AllowNewMenuItems { get; set; } = true;
        public bool AllowSpecialEvents { get; set; } = true;
        public bool AllowDiscountOffers { get; set; } = true;
        public bool AllowOrderReminders { get; set; } = true;
        public NotificationFrequency PreferredFrequency { get; set; } = NotificationFrequency.Daily;
        public List<DayOfWeek> PreferredDays { get; set; } = new List<DayOfWeek>();
        public TimeSpan? PreferredTimeStart { get; set; }
        public TimeSpan? PreferredTimeEnd { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Update customer restaurant notification preferences
    /// </summary>
    public class UpdateCustomerRestaurantNotificationPreferencesDto
    {
        public Guid RestaurantId { get; set; }
        public bool? AllowPromotionalOffers { get; set; }
        public bool? AllowNewMenuItems { get; set; }
        public bool? AllowSpecialEvents { get; set; }
        public bool? AllowDiscountOffers { get; set; }
        public bool? AllowOrderReminders { get; set; }
        public NotificationFrequency? PreferredFrequency { get; set; }
        public List<DayOfWeek> PreferredDays { get; set; }
        public TimeSpan? PreferredTimeStart { get; set; }
        public TimeSpan? PreferredTimeEnd { get; set; }
    }

    /// <summary>
    /// Restaurant notification template
    /// </summary>
    public class RestaurantNotificationTemplate
    {
        public string Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TitleTemplate { get; set; }
        public string MessageTemplate { get; set; }
        public string ImageUrlTemplate { get; set; }
        public NotificationType Type { get; set; }
        public CustomerSegment TargetSegment { get; set; }
        public Dictionary<string, string> DefaultData { get; set; } = new Dictionary<string, string>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UsageCount { get; set; }
        public double AverageEngagement { get; set; }
    }

    /// <summary>
    /// Create restaurant notification template
    /// </summary>
    public class CreateRestaurantNotificationTemplateDto
    {
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string TitleTemplate { get; set; }
        
        [Required]
        public string MessageTemplate { get; set; }
        
        public string ImageUrlTemplate { get; set; }
        
        [Required]
        public NotificationType Type { get; set; }
        
        [Required]
        public CustomerSegment TargetSegment { get; set; }
        
        public Dictionary<string, string> DefaultData { get; set; } = new Dictionary<string, string>();
    }
}
