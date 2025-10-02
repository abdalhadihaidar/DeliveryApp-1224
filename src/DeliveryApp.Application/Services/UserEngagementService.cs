using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Services;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Application.Services
{
    public class UserEngagementService : IUserEngagementService
    {
        private readonly ILogger<UserEngagementService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRepository<UserEngagementMetric> _engagementRepository;
        private readonly IRepository<NotificationPreference> _preferencesRepository;
        private readonly IRepository<UserActivity> _activityRepository;
        private readonly IRepository<EngagementCampaign> _campaignRepository;
        private readonly INotificationService _notificationService;
        private readonly IAnalyticsService _analyticsService;

        public UserEngagementService(
            ILogger<UserEngagementService> logger,
            IConfiguration configuration,
            IRepository<UserEngagementMetric> engagementRepository,
            IRepository<NotificationPreference> preferencesRepository,
            IRepository<UserActivity> activityRepository,
            IRepository<EngagementCampaign> campaignRepository,
            INotificationService notificationService,
            IAnalyticsService analyticsService)
        {
            _logger = logger;
            _configuration = configuration;
            _engagementRepository = engagementRepository;
            _preferencesRepository = preferencesRepository;
            _activityRepository = activityRepository;
            _campaignRepository = campaignRepository;
            _notificationService = notificationService;
            _analyticsService = analyticsService;
        }

        public async Task<NotificationPreference> GetNotificationPreferencesAsync(string userId)
        {
            try
            {
                var preferences = await _preferencesRepository.GetListAsync(p => p.UserId == userId);
                var userPreference = preferences.FirstOrDefault();

                if (userPreference == null)
                {
                    userPreference = new NotificationPreference
                    {
                       
                        UserId = userId,
                        EmailNotifications = true,
                        PushNotifications = true,
                        SmsNotifications = false,
                        OrderUpdates = true,
                        PromotionalOffers = true,
                        NewsAndUpdates = false,
                        RestaurantUpdates = true,
                        DeliveryUpdates = true,
                        PaymentNotifications = true,
                        SecurityAlerts = true,
                        WeeklyDigest = false,
                        QuietHoursEnabled = false,
                        QuietHoursStart = TimeSpan.FromHours(22),
                        QuietHoursEnd = TimeSpan.FromHours(8),
                        PreferredLanguage = "en",
                        TimeZone = "UTC",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _preferencesRepository.InsertAsync(userPreference);
                }

                return userPreference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateNotificationPreferencesAsync(string userId, UpdateNotificationPreferencesDto dto)
        {
            try
            {
                var preferences = await GetNotificationPreferencesAsync(userId);

                preferences.EmailNotifications = dto.EmailNotifications ?? preferences.EmailNotifications;
                preferences.PushNotifications = dto.PushNotifications ?? preferences.PushNotifications;
                preferences.SmsNotifications = dto.SmsNotifications ?? preferences.SmsNotifications;
                preferences.OrderUpdates = dto.OrderUpdates ?? preferences.OrderUpdates;
                preferences.PromotionalOffers = dto.PromotionalOffers ?? preferences.PromotionalOffers;
                preferences.NewsAndUpdates = dto.NewsAndUpdates ?? preferences.NewsAndUpdates;
                preferences.RestaurantUpdates = dto.RestaurantUpdates ?? preferences.RestaurantUpdates;
                preferences.DeliveryUpdates = dto.DeliveryUpdates ?? preferences.DeliveryUpdates;
                preferences.PaymentNotifications = dto.PaymentNotifications ?? preferences.PaymentNotifications;
                preferences.SecurityAlerts = dto.SecurityAlerts ?? preferences.SecurityAlerts;
                preferences.WeeklyDigest = dto.WeeklyDigest ?? preferences.WeeklyDigest;
                preferences.QuietHoursEnabled = dto.QuietHoursEnabled ?? preferences.QuietHoursEnabled;
                preferences.QuietHoursStart = dto.QuietHoursStart ?? preferences.QuietHoursStart;
                preferences.QuietHoursEnd = dto.QuietHoursEnd ?? preferences.QuietHoursEnd;
                preferences.PreferredLanguage = dto.PreferredLanguage ?? preferences.PreferredLanguage;
                preferences.TimeZone = dto.TimeZone ?? preferences.TimeZone;
                preferences.UpdatedAt = DateTime.UtcNow;

                await _preferencesRepository.UpdateAsync(preferences);

                await TrackUserActivityAsync(userId, "notification_preferences_updated", new Dictionary<string, object>
                {
                    { "updated_fields", dto.GetUpdatedFields() },
                    { "timestamp", DateTime.UtcNow }
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for user {UserId}", userId);
                return false;
            }
        }

        public async Task<UserEngagementMetrics> GetUserEngagementMetricsAsync(string userId, EngagementMetricsFilter filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                var activities = await _activityRepository.GetListAsync(
                    a => a.UserId == userId && a.Timestamp >= startDate && a.Timestamp <= endDate
                );

                // Order by timestamp descending in memory
                activities = activities.OrderByDescending(a => a.Timestamp).ToList();

                var metrics = new UserEngagementMetrics
                {
                    UserId = userId,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TotalSessions = activities.Count(a => a.ActivityType == "session_start"),
                    TotalScreenViews = activities.Count(a => a.ActivityType == "screen_view"),
                    TotalOrders = activities.Count(a => a.ActivityType == "order_placed"),
                    TotalTimeSpent = CalculateTotalTimeSpent(activities),
                    AverageSessionDuration = CalculateAverageSessionDuration(activities),
                    LastActiveDate = activities.OrderByDescending(a => a.Timestamp).FirstOrDefault()?.Timestamp,
                    EngagementScore = CalculateEngagementScore(activities),
                    RetentionRate = await CalculateRetentionRateAsync(userId, startDate, endDate),
                    ConversionRate = CalculateConversionRate(activities),
                    ActivityBreakdown = activities.GroupBy(a => a.ActivityType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    DailyActivity = activities.GroupBy(a => a.Timestamp.Date)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    DeviceBreakdown = activities.GroupBy(a => a.DeviceType ?? "unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ChannelBreakdown = activities.GroupBy(a => a.Channel ?? "direct")
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engagement metrics for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> TrackUserActivityAsync(string userId, string activityType, Dictionary<string, object> metadata = null)
        {
            try
            {
                var activity = new UserActivity
                {
                   
                    UserId = userId,
                    ActivityType = activityType,
                    Timestamp = DateTime.UtcNow,
                    Metadata = metadata ?? new Dictionary<string, object>(),
                    DeviceType = metadata?.GetValueOrDefault("device_type")?.ToString(),
                    Platform = metadata?.GetValueOrDefault("platform")?.ToString(),
                    AppVersion = metadata?.GetValueOrDefault("app_version")?.ToString(),
                    Channel = metadata?.GetValueOrDefault("channel")?.ToString(),
                    SessionId = metadata?.GetValueOrDefault("session_id")?.ToString(),
                    IpAddress = metadata?.GetValueOrDefault("ip_address")?.ToString(),
                    UserAgent = metadata?.GetValueOrDefault("user_agent")?.ToString()
                };

                await _activityRepository.InsertAsync(activity);

                // Update real-time engagement metrics
                await UpdateRealTimeEngagementAsync(userId, activityType);

                // Trigger automated campaigns if applicable
                await TriggerAutomatedCampaignsAsync(userId, activityType, metadata);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user activity for user {UserId}, activity {ActivityType}", userId, activityType);
                return false;
            }
        }

        public async Task<List<EngagementCampaign>> GetActiveEngagementCampaignsAsync(string userId)
        {
            try
            {
                var campaigns = await _campaignRepository.GetListAsync(
                    c => c.IsActive && 
                         c.StartDate <= DateTime.UtcNow && 
                         c.EndDate >= DateTime.UtcNow &&
                         (c.TargetUserIds.Contains(userId) || c.TargetUserIds.Count == 0)
                );

                // Order by priority in memory
                return campaigns.OrderBy(c => c.Priority).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active engagement campaigns for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CreateEngagementCampaignAsync(CreateEngagementCampaignDto dto)
        {
            try
            {
                var campaign = new EngagementCampaign
                {
                  
                    Name = dto.Name,
                    Description = dto.Description,
                    CampaignType = dto.CampaignType,
                    TriggerType = dto.TriggerType,
                    TriggerConditions = dto.TriggerConditions,
                    TargetUserIds = dto.TargetUserIds ?? new List<string>(),
                    TargetSegments = dto.TargetSegments ?? new List<string>(),
                    MessageTemplate = dto.MessageTemplate,
                    ActionType = dto.ActionType,
                    ActionData = dto.ActionData ?? new Dictionary<string, object>(),
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Priority = dto.Priority,
                    MaxExecutions = dto.MaxExecutions,
                    CooldownPeriod = dto.CooldownPeriod,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy,
                    Metadata = dto.Metadata ?? new Dictionary<string, object>()
                };

                await _campaignRepository.InsertAsync(campaign);

                _logger.LogInformation("Created engagement campaign {CampaignId} - {CampaignName}", campaign.Id, campaign.Name);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating engagement campaign {CampaignName}", dto.Name);
                return false;
            }
        }

        public async Task<EngagementAnalyticsDto> GetEngagementAnalyticsAsync(EngagementAnalyticsFilter filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                var activities = await _activityRepository.GetListAsync(
                    a => a.Timestamp >= startDate && a.Timestamp <= endDate
                );

                // Order by timestamp descending in memory
                activities = activities.OrderByDescending(a => a.Timestamp).ToList();

                var uniqueUsers = activities.Select(a => a.UserId).Distinct().Count();
                var totalSessions = activities.Count(a => a.ActivityType == "session_start");
                var totalOrders = activities.Count(a => a.ActivityType == "order_placed");

                var analyticsDto = new EngagementAnalyticsDto
                {
                    CalculatedAt = DateTime.UtcNow,
                    ParametersJson = string.Empty, // Fill as needed
                    ResultsJson = string.Empty, // Fill as needed
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TotalUsers = uniqueUsers,
                    ActiveUsers = uniqueUsers,
                    TotalSessions = totalSessions,
                    TotalPageViews = activities.Count(a => a.ActivityType == "screen_view"),
                    TotalOrders = totalOrders,
                    ConversionRate = totalSessions > 0 ? (double)totalOrders / totalSessions * 100 : 0,
                    AverageSessionDuration = CalculateAverageSessionDuration(activities),
                    BounceRate = CalculateBounceRate(activities),
                    RetentionRates = await CalculateRetentionRatesAsync(startDate, endDate),
                    TopActivities = activities.GroupBy(a => a.ActivityType)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    DailyActiveUsers = activities.GroupBy(a => a.Timestamp.Date)
                        .ToDictionary(g => g.Key, g => g.Select(a => a.UserId).Distinct().Count()),
                    DeviceBreakdown = activities.GroupBy(a => a.DeviceType ?? "unknown")
                        .ToDictionary(g => g.Key, g => g.Select(a => a.UserId).Distinct().Count()),
                    PlatformBreakdown = activities.GroupBy(a => a.Platform ?? "unknown")
                        .ToDictionary(g => g.Key, g => g.Select(a => a.UserId).Distinct().Count()),
                    ChannelBreakdown = activities.GroupBy(a => a.Channel ?? "direct")
                        .ToDictionary(g => g.Key, g => g.Select(a => a.UserId).Distinct().Count())
                };

                return analyticsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engagement analytics");
                throw;
            }
        }

        public async Task<bool> ScheduleNotificationAsync(ScheduleNotificationDto dto)
        {
            try
            {
                var notification = new ScheduledNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = dto.Title,
                    Body = dto.Body,
                    Type = dto.Type,
                    Data = dto.Data ?? new Dictionary<string, object>(),
                    RecipientIds = dto.RecipientIds ?? new List<string>(),
                    ScheduledAt = dto.ScheduledAt,
                    TimeZone = dto.TimeZone ?? "UTC",
                    IsRecurring = dto.IsRecurring,
                    RecurrencePattern = dto.RecurrencePattern,
                    ExpiresAt = dto.ExpiresAt,
                    Priority = dto.Priority,
                    ImageUrl = dto.ImageUrl,
                    ActionUrl = dto.ActionUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy,
                    Metadata = dto.Metadata ?? new Dictionary<string, object>()
                };

                await _notificationService.ScheduleNotificationAsync(notification);

                _logger.LogInformation("Scheduled notification {NotificationId} for {ScheduledAt}", notification.Id, notification.ScheduledAt);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notification");
                return false;
            }
        }

        public async Task<PersonalizationProfile> GetPersonalizationProfileAsync(string userId)
        {
            try
            {
                var activities = await _activityRepository.GetListAsync(
                    a => a.UserId == userId
                );

                // Order by timestamp descending and take top 1000 in memory
                activities = activities.OrderByDescending(a => a.Timestamp).Take(1000).ToList();

                var preferences = await GetNotificationPreferencesAsync(userId);

                var profile = new PersonalizationProfile
                {
                    UserId = userId,
                    PreferredCategories = ExtractPreferredCategories(activities),
                    PreferredRestaurants = ExtractPreferredRestaurants(activities),
                    PreferredOrderTimes = ExtractPreferredOrderTimes(activities),
                    PreferredPaymentMethods = ExtractPreferredPaymentMethods(activities),
                    AverageOrderValue = CalculateAverageOrderValue(activities),
                    OrderFrequency = CalculateOrderFrequency(activities),
                    PreferredLanguage = preferences.PreferredLanguage,
                    TimeZone = preferences.TimeZone,
                    DevicePreferences = ExtractDevicePreferences(activities),
                    EngagementLevel = CalculateEngagementLevel(activities),
                    LastUpdated = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>()
                };

                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personalization profile for user {UserId}", userId);
                throw;
            }
        }

        private async Task UpdateRealTimeEngagementAsync(string userId, string activityType)
        {
            // Update real-time engagement metrics in cache or database
            await _analyticsService.UpdateRealTimeMetricAsync($"user_activity:{userId}", activityType);
        }

        private async Task TriggerAutomatedCampaignsAsync(string userId, string activityType, Dictionary<string, object> metadata)
        {
            var campaigns = await GetActiveEngagementCampaignsAsync(userId);
            
            foreach (var campaign in campaigns.Where(c => c.TriggerType == "activity" && 
                                                         c.TriggerConditions.ContainsKey("activity_type") &&
                                                         c.TriggerConditions["activity_type"].ToString() == activityType))
            {
                await ExecuteCampaignAsync(campaign, userId, metadata);
            }
        }

        private async Task ExecuteCampaignAsync(EngagementCampaign campaign, string userId, Dictionary<string, object> context)
        {
            try
            {
                // Check execution limits and cooldown
                if (!await CanExecuteCampaignAsync(campaign, userId))
                {
                    return;
                }

                // Execute campaign action
                switch (campaign.ActionType)
                {
                    case "send_notification":
                        await SendCampaignNotificationAsync(campaign, userId, context);
                        break;
                    case "send_email":
                        await SendCampaignEmailAsync(campaign, userId, context);
                        break;
                    case "show_popup":
                        await TriggerInAppPopupAsync(campaign, userId, context);
                        break;
                    case "apply_discount":
                        await ApplyDiscountAsync(campaign, userId, context);
                        break;
                }

                // Track campaign execution
                await TrackCampaignExecutionAsync(campaign, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing campaign {CampaignId} for user {UserId}", campaign.Id, userId);
            }
        }

        private async Task<bool> CanExecuteCampaignAsync(EngagementCampaign campaign, string userId)
        {
            // Implementation for checking execution limits and cooldown
            return true; // Simplified for now
        }

        private async Task SendCampaignNotificationAsync(EngagementCampaign campaign, string userId, Dictionary<string, object> context)
        {
            var notification = new CreateNotificationDto
            {
                Title = campaign.MessageTemplate.Title,
                Body = campaign.MessageTemplate.Body,
                Type = Contracts.Dtos.NotificationType.General,
                RecipientIds = new List<string> { userId },
                Data = campaign.ActionData
            };

            await _notificationService.SendNotificationAsync(notification);
        }

        private async Task SendCampaignEmailAsync(EngagementCampaign campaign, string userId, Dictionary<string, object> context)
        {
            // Implementation for sending campaign emails
            await Task.CompletedTask;
        }

        private async Task TriggerInAppPopupAsync(EngagementCampaign campaign, string userId, Dictionary<string, object> context)
        {
            // Implementation for triggering in-app popups
            await Task.CompletedTask;
        }

        private async Task ApplyDiscountAsync(EngagementCampaign campaign, string userId, Dictionary<string, object> context)
        {
            // Implementation for applying discounts
            await Task.CompletedTask;
        }

        private async Task TrackCampaignExecutionAsync(EngagementCampaign campaign, string userId)
        {
            await TrackUserActivityAsync(userId, "campaign_executed", new Dictionary<string, object>
            {
                { "campaign_id", campaign.Id },
                { "campaign_name", campaign.Name },
                { "campaign_type", campaign.CampaignType },
                { "execution_time", DateTime.UtcNow }
            });
        }

        private double CalculateTotalTimeSpent(IEnumerable<UserActivity> activities)
        {
            // Implementation for calculating total time spent
            return activities.Count() * 2.5; // Simplified calculation
        }

        private double CalculateAverageSessionDuration(IEnumerable<UserActivity> activities)
        {
            // Implementation for calculating average session duration
            var sessions = activities.Where(a => a.ActivityType == "session_start").Count();
            return sessions > 0 ? CalculateTotalTimeSpent(activities) / sessions : 0;
        }

        private double CalculateEngagementScore(IEnumerable<UserActivity> activities)
        {
            // Implementation for calculating engagement score
            var score = activities.Count() * 10;
            var orders = activities.Count(a => a.ActivityType == "order_placed");
            score += orders * 50;
            return Math.Min(score, 1000); // Cap at 1000
        }

        private async Task<double> CalculateRetentionRateAsync(string userId, DateTime startDate, DateTime endDate)
        {
            // Implementation for calculating retention rate
            return 75.0; // Simplified for now
        }

        private double CalculateConversionRate(IEnumerable<UserActivity> activities)
        {
            var sessions = activities.Count(a => a.ActivityType == "session_start");
            var orders = activities.Count(a => a.ActivityType == "order_placed");
            return sessions > 0 ? (double)orders / sessions * 100 : 0;
        }

        private double CalculateBounceRate(IEnumerable<UserActivity> activities)
        {
            // Implementation for calculating bounce rate
            return 25.0; // Simplified for now
        }

        private async Task<Dictionary<string, double>> CalculateRetentionRatesAsync(DateTime startDate, DateTime endDate)
        {
            // Implementation for calculating retention rates
            return new Dictionary<string, double>
            {
                { "day_1", 85.0 },
                { "day_7", 65.0 },
                { "day_30", 45.0 }
            };
        }

        private List<string> ExtractPreferredCategories(IEnumerable<UserActivity> activities)
        {
            return activities
                .Where(a => a.Metadata.ContainsKey("category"))
                .GroupBy(a => a.Metadata["category"].ToString())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
        }

        private List<string> ExtractPreferredRestaurants(IEnumerable<UserActivity> activities)
        {
            return activities
                .Where(a => a.Metadata.ContainsKey("restaurant_id"))
                .GroupBy(a => a.Metadata["restaurant_id"].ToString())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
        }

        private List<TimeSpan> ExtractPreferredOrderTimes(IEnumerable<UserActivity> activities)
        {
            return activities
                .Where(a => a.ActivityType == "order_placed")
                .GroupBy(a => a.Timestamp.TimeOfDay.Hours)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => TimeSpan.FromHours(g.Key))
                .ToList();
        }

        private List<string> ExtractPreferredPaymentMethods(IEnumerable<UserActivity> activities)
        {
            return activities
                .Where(a => a.Metadata.ContainsKey("payment_method"))
                .GroupBy(a => a.Metadata["payment_method"].ToString())
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToList();
        }

        private double CalculateAverageOrderValue(IEnumerable<UserActivity> activities)
        {
            var orders = activities.Where(a => a.ActivityType == "order_placed" && a.Metadata.ContainsKey("order_value"));
            if (!orders.Any()) return 0;

            return orders.Average(a => Convert.ToDouble(a.Metadata["order_value"]));
        }

        private double CalculateOrderFrequency(IEnumerable<UserActivity> activities)
        {
            var orders = activities.Where(a => a.ActivityType == "order_placed").ToList();
            if (orders.Count < 2) return 0;

            var timeSpan = orders.Max(a => a.Timestamp) - orders.Min(a => a.Timestamp);
            return timeSpan.TotalDays > 0 ? orders.Count / timeSpan.TotalDays : 0;
        }

        private Dictionary<string, int> ExtractDevicePreferences(IEnumerable<UserActivity> activities)
        {
            return activities
                .GroupBy(a => a.DeviceType ?? "unknown")
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private string CalculateEngagementLevel(IEnumerable<UserActivity> activities)
        {
            var score = CalculateEngagementScore(activities);
            
            if (score >= 800) return "high";
            if (score >= 400) return "medium";
            return "low";
        }
    }
}

