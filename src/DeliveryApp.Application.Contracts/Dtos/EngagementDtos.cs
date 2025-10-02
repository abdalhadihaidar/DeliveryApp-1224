using System;
using System.Collections.Generic;
using DeliveryApp.Domain.Entities;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Filtering options when requesting engagement metrics for a user.
    /// </summary>
    public class EngagementMetricsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Filtering options when requesting aggregate engagement analytics.
    /// </summary>
    public class EngagementAnalyticsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// DTO used when creating a new automated engagement campaign.
    /// Mirrors (a subset of) the EngagementCampaign entity properties.
    /// </summary>
    public class CreateEngagementCampaignDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CampaignType { get; set; }
        public string TriggerType { get; set; }
        public Dictionary<string, object> TriggerConditions { get; set; } = new();
        public List<string> TargetUserIds { get; set; } = new();
        public List<string> TargetSegments { get; set; } = new();
        public MessageTemplate MessageTemplate { get; set; } = new();
        public string ActionType { get; set; }
        public Dictionary<string, object> ActionData { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Priority { get; set; } = 0;
        public int? MaxExecutions { get; set; }
        public TimeSpan? CooldownPeriod { get; set; }
        public string CreatedBy { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// DTO used by the engagement service for scheduling an ad-hoc notification to one or more users.
    /// </summary>
    public class ScheduleNotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Custom;
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> RecipientIds { get; set; } = new();
        public DateTime ScheduledAt { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public bool IsRecurring { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public string ImageUrl { get; set; }
        public string ActionUrl { get; set; }
        public string CreatedBy { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// DTO for aggregate engagement analytics, used in API contracts.
    /// </summary>
    public class EngagementAnalyticsDto
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
    }
} 
