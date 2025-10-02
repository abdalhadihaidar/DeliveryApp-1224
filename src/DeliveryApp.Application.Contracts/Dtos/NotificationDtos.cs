using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Notification message DTO
    /// </summary>
    public class NotificationMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public string Sound { get; set; }
        public string ClickAction { get; set; }
        public string Category { get; set; }
        public bool Badge { get; set; }
        public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Notification result DTO
    /// </summary>
    public class NotificationResult
    {
        public bool Success { get; set; }
        public string NotificationId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedTokens { get; set; } = new List<string>();
    }

    /// <summary>
    /// Scheduled notification DTO
    /// </summary>
    public partial class ScheduledNotification
    {
        public string Id { get; set; }
        public NotificationMessage Message { get; set; }
        public List<string> DeviceTokens { get; set; }
        public string Topic { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string TimeZone { get; set; }
        public bool Recurring { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// A/B test notification DTO
    /// </summary>
    public class ABTestNotification
    {
        public string TestId { get; set; }
        public string TestName { get; set; }
        public NotificationMessage VariantA { get; set; }
        public NotificationMessage VariantB { get; set; }
        public List<string> DeviceTokens { get; set; }
        public double SplitPercentage { get; set; } = 50.0;
        public ABTestMetrics Metrics { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// Notification template DTO
    /// </summary>
    public class NotificationTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TitleTemplate { get; set; }
        public string BodyTemplate { get; set; }
        public string ImageUrlTemplate { get; set; }
        public NotificationType Type { get; set; }
        public NotificationCategory Category { get; set; }
        public List<TemplateParameter> Parameters { get; set; } = new List<TemplateParameter>();
        public Dictionary<string, string> DefaultData { get; set; } = new Dictionary<string, string>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public LocalizedContent LocalizedContent { get; set; }
    }


    /// <summary>
    /// Notification preferences DTO
    /// </summary>
    public class NotificationPreferences
    {
        public string UserId { get; set; }
        public bool OrderUpdates { get; set; } = true;
        public bool PromotionalOffers { get; set; } = true;
        public bool RestaurantUpdates { get; set; } = true;
        public bool DeliveryUpdates { get; set; } = true;
        public bool PaymentNotifications { get; set; } = true;
        public bool SecurityAlerts { get; set; } = true;
        public bool NewsAndUpdates { get; set; } = false;
        public bool SurveyRequests { get; set; } = false;
        public NotificationTiming PreferredTiming { get; set; }
        public List<string> MutedTopics { get; set; } = new List<string>();
        public Dictionary<NotificationType, bool> CustomPreferences { get; set; } = new Dictionary<NotificationType, bool>();
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Update notification preferences DTO
    /// </summary>
    public partial class UpdateNotificationPreferencesDto
    {
        public bool? EmailNotifications { get; set; }
        public bool? PushNotifications { get; set; }
        public bool? SmsNotifications { get; set; }
        public bool? OrderUpdates { get; set; }
        public bool? PromotionalOffers { get; set; }
        public bool? RestaurantUpdates { get; set; }
        public bool? DeliveryUpdates { get; set; }
        public bool? PaymentNotifications { get; set; }
        public bool? SecurityAlerts { get; set; }
        public bool? NewsAndUpdates { get; set; }
        public bool? SurveyRequests { get; set; }
        public bool? WeeklyDigest { get; set; }
        public bool? QuietHoursEnabled { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public string PreferredLanguage { get; set; }
        public string TimeZone { get; set; }
        public NotificationTiming? PreferredTiming { get; set; }
        public List<string> MutedTopics { get; set; }
        public Dictionary<NotificationType, bool> CustomPreferences { get; set; }
    }

    /// <summary>
    /// Notification settings DTO
    /// </summary>
    public class NotificationSettings
    {
        public string UserId { get; set; }
        public bool PushNotificationsEnabled { get; set; } = true;
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool SmsNotificationsEnabled { get; set; } = false;
        public bool InAppNotificationsEnabled { get; set; } = true;
        public string TimeZone { get; set; }
        public TimeSpan QuietHoursStart { get; set; }
        public TimeSpan QuietHoursEnd { get; set; }
        public List<DayOfWeek> QuietDays { get; set; } = new List<DayOfWeek>();
        public NotificationFrequency Frequency { get; set; } = NotificationFrequency.Immediate;
        public string Language { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Notification statistics DTO
    /// </summary>
    public class NotificationStatistics
    {
        public string NotificationId { get; set; }
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        public int TotalFailed { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        public DateTime SentAt { get; set; }
        public Dictionary<string, int> PlatformBreakdown { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ErrorBreakdown { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// Template statistics DTO
    /// </summary>
    public class TemplateStatistics
    {
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public int TotalUsage { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public double AverageOpenRate { get; set; }
        public double AverageClickRate { get; set; }
        public DateTime FirstUsed { get; set; }
        public DateTime LastUsed { get; set; }
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Notification tracking data DTO
    /// </summary>
    public class NotificationTrackingData
    {
        public string NotificationId { get; set; }
        public string UserId { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
        public NotificationType Type { get; set; }
        public string TemplateId { get; set; }
        public string CampaignId { get; set; }
        public DateTime SentAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Notification analytics DTO
    /// </summary>
    public class NotificationAnalytics
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalNotifications { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        public double OverallDeliveryRate { get; set; }
        public double OverallOpenRate { get; set; }
        public double OverallClickRate { get; set; }
        public Dictionary<NotificationType, NotificationTypeMetrics> TypeBreakdown { get; set; } = new Dictionary<NotificationType, NotificationTypeMetrics>();
        public Dictionary<string, PlatformMetrics> PlatformBreakdown { get; set; } = new Dictionary<string, PlatformMetrics>();
        public List<DailyMetrics> DailyMetrics { get; set; } = new List<DailyMetrics>();
    }

    /// <summary>
    /// User engagement metrics DTO
    /// </summary>
    public partial class UserEngagementMetrics
    {
        public string UserId { get; set; }
        public int TotalNotificationsReceived { get; set; }
        public int TotalNotificationsOpened { get; set; }
        public int TotalNotificationsClicked { get; set; }
        public double EngagementScore { get; set; }
        public DateTime LastEngagement { get; set; }
        public Dictionary<NotificationType, int> TypeEngagement { get; set; } = new Dictionary<NotificationType, int>();
        public List<EngagementTrend> EngagementTrends { get; set; } = new List<EngagementTrend>();
        public UserSegment Segment { get; set; }
    }

    /// <summary>
    /// Campaign performance DTO
    /// </summary>
    public class CampaignPerformance
    {
        public string CampaignId { get; set; }
        public string CampaignName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalTargeted { get; set; }
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        public double ConversionRate { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal ROI { get; set; }
        public Dictionary<string, object> CustomMetrics { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Supporting enums and classes
    /// </summary>
    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

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

    public enum NotificationCategory
    {
        Transactional,
        Marketing,
        System,
        Support,
        Emergency
    }

    public enum NotificationFrequency
    {
        Immediate,
        Hourly,
        Daily,
        Weekly,
        Custom
    }

    public class NotificationTiming
    {
        public TimeSpan PreferredStartTime { get; set; }
        public TimeSpan PreferredEndTime { get; set; }
        public List<DayOfWeek> PreferredDays { get; set; } = new List<DayOfWeek>();
        public string TimeZone { get; set; }
    }

    public class RecurrencePattern
    {
        public RecurrenceType Type { get; set; }
        public int Interval { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; } = new List<DayOfWeek>();
        public int? DayOfMonth { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxOccurrences { get; set; }
    }

    public enum RecurrenceType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public class ABTestMetrics
    {
        public string VariantAId { get; set; }
        public string VariantBId { get; set; }
        public int VariantASent { get; set; }
        public int VariantBSent { get; set; }
        public int VariantAOpened { get; set; }
        public int VariantBOpened { get; set; }
        public int VariantAClicked { get; set; }
        public int VariantBClicked { get; set; }
        public double VariantAConversion { get; set; }
        public double VariantBConversion { get; set; }
        public string WinningVariant { get; set; }
        public double ConfidenceLevel { get; set; }
    }

    public class TemplateParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public object DefaultValue { get; set; }
        public List<string> AllowedValues { get; set; } = new List<string>();
    }

    public class LocalizedContent
    {
        public Dictionary<string, string> Titles { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Bodies { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ImageUrls { get; set; } = new Dictionary<string, string>();
    }

    public class NotificationTypeMetrics
    {
        public NotificationType Type { get; set; }
        public int Count { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    public class PlatformMetrics
    {
        public string Platform { get; set; }
        public int Count { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    public class DailyMetrics
    {
        public DateTime Date { get; set; }
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public double DeliveryRate { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    public class EngagementTrend
    {
        public DateTime Date { get; set; }
        public double EngagementScore { get; set; }
        public int NotificationsReceived { get; set; }
        public int NotificationsOpened { get; set; }
        public int NotificationsClicked { get; set; }
    }

    public enum UserSegment
    {
        HighlyEngaged,
        ModeratelyEngaged,
        LowEngaged,
        Inactive,
        New,
        Churned
    }

    public class NotificationAnalyticsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<NotificationType> Types { get; set; } = new List<NotificationType>();
        public List<string> Platforms { get; set; } = new List<string>();
        public List<string> CampaignIds { get; set; } = new List<string>();
        public List<string> TemplateIds { get; set; } = new List<string>();
        public string UserId { get; set; }
        public bool IncludeABTests { get; set; }
    }

    public class NotificationReport
    {
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public NotificationReportType Type { get; set; }
        public NotificationAnalytics Analytics { get; set; }
        public List<CampaignPerformance> CampaignPerformances { get; set; } = new List<CampaignPerformance>();
        public List<TemplateStatistics> TemplateStatistics { get; set; } = new List<TemplateStatistics>();
        public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
        public string GeneratedBy { get; set; }
    }

    public class NotificationReportRequest
    {
        public string ReportName { get; set; }
        public NotificationReportType Type { get; set; }
        public NotificationAnalyticsFilter Filter { get; set; }
        public List<string> IncludeMetrics { get; set; } = new List<string>();
        public string Format { get; set; } = "JSON";
        public bool IncludeCharts { get; set; }
        public string DeliveryMethod { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
    }

    public enum NotificationReportType
    {
        Summary,
        Detailed,
        Campaign,
        Template,
        UserEngagement,
        ABTest,
        Custom
    }

    public class UserNotificationPreference
    {
        public string UserId { get; set; }
        public NotificationPreferences Preferences { get; set; }
    }
}

