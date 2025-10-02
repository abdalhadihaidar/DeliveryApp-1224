using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace DeliveryApp.Domain.Entities
{
    /// <summary>
    /// Stores a single user activity event (analytics). Only minimal structure needed for compilation.
    /// </summary>
    public class UserActivity : Entity<string>
    {
        public string UserId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();

        // Optional contextual fields referenced by the application layer
        public string? DeviceType { get; set; }
        public string? Platform { get; set; }
        public string? AppVersion { get; set; }
        public string? Channel { get; set; }
        public string? SessionId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public override object[] GetKeys() => new object[] { Id };
    }

    /// <summary>
    /// Aggregated engagement metrics snapshot for a user.
    /// </summary>
    public class UserEngagementMetric : Entity<string>
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string JsonPayload { get; set; } = string.Empty; // blob storage of complex metrics
        public override object[] GetKeys() => new object[] { Id };
    }

    /// <summary>
    /// User's notification channel/feature preferences.
    /// </summary>
    public class NotificationPreference : Entity<string>
    {
        public string UserId { get; set; } = string.Empty;

        // Boolean flags (superset of those used in the service).
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool OrderUpdates { get; set; }
        public bool PromotionalOffers { get; set; }
        public bool NewsAndUpdates { get; set; }
        public bool RestaurantUpdates { get; set; }
        public bool DeliveryUpdates { get; set; }
        public bool PaymentNotifications { get; set; }
        public bool SecurityAlerts { get; set; }
        public bool WeeklyDigest { get; set; }
        public bool QuietHoursEnabled { get; set; }
        public TimeSpan QuietHoursStart { get; set; }
        public TimeSpan QuietHoursEnd { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public string TimeZone { get; set; } = "UTC";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override object[] GetKeys() => new object[] { Id };
    }

    /// <summary>
    /// Definition for an automated engagement/burst campaign.
    /// </summary>
    public class EngagementCampaign : Entity<string>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public string ConditionsJson { get; set; } = string.Empty; // serialized rule set
        public string ActionsJson { get; set; } = string.Empty; // serialized actions
        public string CampaignType { get; set; } = string.Empty;
        public string TriggerType { get; set; } = string.Empty;
        public Dictionary<string, object> TriggerConditions { get; set; } = new();
        public List<string> TargetUserIds { get; set; } = new();
        public List<string> TargetSegments { get; set; } = new();
        public MessageTemplate MessageTemplate { get; set; } = new();
        public string ActionType { get; set; } = string.Empty;
        public Dictionary<string, object> ActionData { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Priority { get; set; }
        public int? MaxExecutions { get; set; }
        public TimeSpan? CooldownPeriod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public override object[] GetKeys() => new object[] { Id };
    }

    /// <summary>
    /// Personalized segmentation info for a user used in recommendation / targeting.
    /// </summary>
    public class PersonalizationProfile : Entity<string>
    {
        public string UserId { get; set; } = string.Empty;
        public string SegmentsJson { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public List<string> PreferredCategories { get; set; } = new();
        public List<string> PreferredRestaurants { get; set; } = new();
        public List<TimeSpan> PreferredOrderTimes { get; set; } = new();
        public List<string> PreferredPaymentMethods { get; set; } = new();
        public double AverageOrderValue { get; set; }
        public double OrderFrequency { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public string TimeZone { get; set; } = "UTC";
        public Dictionary<string, int> DevicePreferences { get; set; } = new();
        public string EngagementLevel { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public override object[] GetKeys() => new object[] { Id };
    }

    /// <summary>
    /// Snapshot object for various analytic KPIs; kept as JSON for flexibility.
    /// </summary>
    public class EngagementAnalytics : Entity<string>
    {
        public DateTime CalculatedAt { get; set; }
        public string ParametersJson { get; set; } = string.Empty;
        public string ResultsJson { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalSessions { get; set; }
        public int TotalPageViews { get; set; }
        public int TotalOrders { get; set; }
        public double ConversionRate { get; set; }
        public double AverageSessionDuration { get; set; }
        public double BounceRate { get; set; }
        public Dictionary<string, double> RetentionRates { get; set; } = new();
        public Dictionary<string, int> TopActivities { get; set; } = new();
        public Dictionary<DateTime, int> DailyActiveUsers { get; set; } = new();
        public Dictionary<string, int> DeviceBreakdown { get; set; } = new();
        public Dictionary<string, int> PlatformBreakdown { get; set; } = new();
        public Dictionary<string, int> ChannelBreakdown { get; set; } = new();
        public override object[] GetKeys() => new object[] { Id };
    }

    /// <summary>
    /// Lightweight message template with title & body used by engagement campaigns.
    /// Placed in the domain layer to avoid references to application-layer DTOs.
    /// </summary>
    public class MessageTemplate
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
} 
