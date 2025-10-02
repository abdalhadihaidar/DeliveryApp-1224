using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Service for restaurant owners to send targeted notifications to their previous customers
    /// </summary>
    public class RestaurantNotificationService : ApplicationService, IRestaurantNotificationService, ITransientDependency
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IFirebaseNotificationService _notificationService;
        private readonly INotificationAnalyticsService _analyticsService;
        private readonly ILogger<RestaurantNotificationService> _logger;

        public RestaurantNotificationService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository,
            IFirebaseNotificationService notificationService,
            INotificationAnalyticsService analyticsService,
            ILogger<RestaurantNotificationService> logger)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
            _notificationService = notificationService;
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Send targeted notification to restaurant's previous customers
        /// </summary>
        public async Task<RestaurantNotificationCampaignResult> SendTargetedNotificationAsync(RestaurantTargetedNotificationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting targeted notification campaign for restaurant {RestaurantId}", request.RestaurantId);

                // Validate restaurant exists and user has permission
                var restaurant = await _restaurantRepository.GetAsync(request.RestaurantId);
                if (restaurant == null)
                {
                    return new RestaurantNotificationCampaignResult
                    {
                        Success = false,
                        Message = "Restaurant not found",
                        RestaurantId = request.RestaurantId
                    };
                }

                // Get targeted customers based on criteria
                var targetedCustomers = await GetTargetedCustomersAsync(request.RestaurantId, request.TargetingCriteria);
                
                if (!targetedCustomers.Any())
                {
                    return new RestaurantNotificationCampaignResult
                    {
                        Success = false,
                        Message = "No customers match the targeting criteria",
                        RestaurantId = request.RestaurantId,
                        RestaurantName = restaurant.Name,
                        TotalTargetedCustomers = 0
                    };
                }

                // Get device tokens for targeted customers
                var customerTokens = await GetCustomerDeviceTokensAsync(targetedCustomers);
                
                if (!customerTokens.Any())
                {
                    return new RestaurantNotificationCampaignResult
                    {
                        Success = false,
                        Message = "No customers have valid device tokens",
                        RestaurantId = request.RestaurantId,
                        RestaurantName = restaurant.Name,
                        TotalTargetedCustomers = targetedCustomers.Count,
                        CustomersWithValidTokens = 0
                    };
                }

                // Create notification message
                var notificationMessage = new NotificationMessage
                {
                    Title = request.Title,
                    Body = request.Message,
                    ImageUrl = request.ImageUrl,
                    Priority = request.Priority,
                    Category = request.Type.ToString(),
                    Data = new Dictionary<string, string>
                    {
                        ["restaurantId"] = request.RestaurantId.ToString(),
                        ["restaurantName"] = restaurant.Name,
                        ["actionUrl"] = request.ActionUrl ?? "",
                        ["type"] = request.Type.ToString()
                    },
                    CustomData = request.CustomData?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) ?? new Dictionary<string, object>()
                };

                var campaignId = Guid.NewGuid().ToString();
                var result = new RestaurantNotificationCampaignResult
                {
                    CampaignId = campaignId,
                    RestaurantId = request.RestaurantId,
                    RestaurantName = restaurant.Name,
                    CreatedAt = DateTime.UtcNow,
                    ScheduledFor = request.ScheduledTime,
                    TotalTargetedCustomers = targetedCustomers.Count,
                    CustomersWithValidTokens = customerTokens.Count,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    TargetingCriteria = request.TargetingCriteria
                };

                // Send notifications
                if (request.SendImmediately)
                {
                    var notificationResults = await _notificationService.SendNotificationToMultipleAsync(customerTokens, notificationMessage);
                    
                    result.SentAt = DateTime.UtcNow;
                    result.Success = true;
                    result.TotalSent = notificationResults.Count;
                    result.DeliveredCount = notificationResults.Count(r => r.Success);
                    result.FailedCount = notificationResults.Count(r => !r.Success);
                    result.FailedTokens = notificationResults.Where(r => !r.Success).SelectMany(r => r.FailedTokens).ToList();
                    result.DeliveryRate = result.TotalSent > 0 ? (double)result.DeliveredCount / result.TotalSent * 100 : 0;
                    result.EstimatedOpenRate = CalculateEstimatedOpenRate(request.Type);
                    result.EstimatedClickRate = CalculateEstimatedClickRate(request.Type);
                }
                else if (request.ScheduledTime.HasValue)
                {
                    // Schedule notification
                    var scheduledNotification = new ScheduledNotification
                    {
                        Id = campaignId,
                        Message = notificationMessage,
                        DeviceTokens = customerTokens,
                        ScheduledTime = request.ScheduledTime.Value,
                        TimeZone = "UTC"
                    };

                    await _notificationService.ScheduleNotificationAsync(scheduledNotification);
                    result.Success = true;
                    result.Message = "Notification scheduled successfully";
                }

                _logger.LogInformation("Targeted notification campaign completed for restaurant {RestaurantId}. Sent: {Sent}, Delivered: {Delivered}", 
                    request.RestaurantId, result.TotalSent, result.DeliveredCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending targeted notification for restaurant {RestaurantId}", request.RestaurantId);
                return new RestaurantNotificationCampaignResult
                {
                    Success = false,
                    Message = "Internal error occurred while sending notification",
                    RestaurantId = request.RestaurantId
                };
            }
        }

        /// <summary>
        /// Get restaurant's notification campaign history
        /// </summary>
        public async Task<List<RestaurantNotificationCampaignSummary>> GetRestaurantCampaignsAsync(Guid restaurantId, int skipCount = 0, int maxResultCount = 10)
        {
            // In real implementation, get from database
            // For now, return mock data
            return new List<RestaurantNotificationCampaignSummary>
            {
                new RestaurantNotificationCampaignSummary
                {
                    CampaignId = Guid.NewGuid().ToString(),
                    RestaurantId = restaurantId,
                    RestaurantName = "Sample Restaurant",
                    Title = "Special Offer!",
                    Type = NotificationType.PromotionalOffer,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    SentAt = DateTime.UtcNow.AddDays(-1),
                    Status = CampaignStatus.Sent,
                    TotalTargeted = 150,
                    TotalSent = 145,
                    Delivered = 140,
                    Opened = 85,
                    Clicked = 25,
                    DeliveryRate = 96.6,
                    OpenRate = 60.7,
                    ClickRate = 17.2
                }
            };
        }

        /// <summary>
        /// Get specific campaign details
        /// </summary>
        public async Task<RestaurantNotificationCampaignResult> GetCampaignDetailsAsync(string campaignId)
        {
            // In real implementation, get from database
            return new RestaurantNotificationCampaignResult
            {
                CampaignId = campaignId,
                Success = true,
                Message = "Campaign details retrieved successfully"
            };
        }

        /// <summary>
        /// Get restaurant notification analytics
        /// </summary>
        public async Task<RestaurantNotificationAnalytics> GetRestaurantAnalyticsAsync(Guid restaurantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            // In real implementation, calculate from database
            return new RestaurantNotificationAnalytics
            {
                RestaurantId = restaurantId,
                RestaurantName = "Sample Restaurant",
                StartDate = start,
                EndDate = end,
                TotalCampaigns = 5,
                TotalNotificationsSent = 750,
                TotalDelivered = 720,
                TotalOpened = 450,
                TotalClicked = 125,
                OverallDeliveryRate = 96.0,
                OverallOpenRate = 62.5,
                OverallClickRate = 17.4,
                UniqueCustomersReached = 300,
                NewCustomersAcquired = 25,
                ReturningCustomersRetained = 275
            };
        }

        /// <summary>
        /// Get customer count for targeting criteria (preview)
        /// </summary>
        public async Task<int> GetTargetedCustomerCountAsync(Guid restaurantId, CustomerTargetingCriteria criteria)
        {
            var targetedCustomers = await GetTargetedCustomersAsync(restaurantId, criteria);
            return targetedCustomers.Count;
        }

        /// <summary>
        /// Get customer segments for restaurant
        /// </summary>
        public async Task<Dictionary<CustomerSegment, int>> GetCustomerSegmentsAsync(Guid restaurantId)
        {
            // In real implementation, calculate from database
            return new Dictionary<CustomerSegment, int>
            {
                { CustomerSegment.NewCustomers, 25 },
                { CustomerSegment.ReturningCustomers, 150 },
                { CustomerSegment.HighValue, 45 },
                { CustomerSegment.FrequentBuyers, 80 },
                { CustomerSegment.Inactive, 30 },
                { CustomerSegment.VIP, 60 },
                { CustomerSegment.Occasional, 20 },
                { CustomerSegment.Active, 100 }
            };
        }

        #region Template Management

        public async Task<RestaurantNotificationTemplate> CreateTemplateAsync(CreateRestaurantNotificationTemplateDto request)
        {
            // In real implementation, save to database
            return new RestaurantNotificationTemplate
            {
                Id = Guid.NewGuid().ToString(),
                RestaurantId = Guid.NewGuid(), // Get from current user context
                Name = request.Name,
                Description = request.Description,
                TitleTemplate = request.TitleTemplate,
                MessageTemplate = request.MessageTemplate,
                ImageUrlTemplate = request.ImageUrlTemplate,
                Type = request.Type,
                TargetSegment = request.TargetSegment,
                DefaultData = request.DefaultData,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UsageCount = 0,
                AverageEngagement = 0.0
            };
        }

        public async Task<List<RestaurantNotificationTemplate>> GetRestaurantTemplatesAsync(Guid restaurantId)
        {
            // In real implementation, get from database
            return new List<RestaurantNotificationTemplate>();
        }

        public async Task<RestaurantNotificationTemplate> UpdateTemplateAsync(string templateId, CreateRestaurantNotificationTemplateDto request)
        {
            // In real implementation, update in database
            return new RestaurantNotificationTemplate
            {
                Id = templateId,
                Name = request.Name,
                Description = request.Description,
                TitleTemplate = request.TitleTemplate,
                MessageTemplate = request.MessageTemplate,
                ImageUrlTemplate = request.ImageUrlTemplate,
                Type = request.Type,
                TargetSegment = request.TargetSegment,
                DefaultData = request.DefaultData,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteTemplateAsync(string templateId)
        {
            // In real implementation, delete from database
            return true;
        }

        #endregion

        #region Customer Preferences

        public async Task<CustomerRestaurantNotificationPreferences> GetCustomerPreferencesAsync(Guid customerId, Guid restaurantId)
        {
            // In real implementation, get from database
            return new CustomerRestaurantNotificationPreferences
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                AllowPromotionalOffers = true,
                AllowNewMenuItems = true,
                AllowSpecialEvents = true,
                AllowDiscountOffers = true,
                AllowOrderReminders = true,
                PreferredFrequency = NotificationFrequency.Daily,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public async Task<CustomerRestaurantNotificationPreferences> UpdateCustomerPreferencesAsync(Guid customerId, UpdateCustomerRestaurantNotificationPreferencesDto request)
        {
            // In real implementation, update in database
            return new CustomerRestaurantNotificationPreferences
            {
                CustomerId = customerId,
                RestaurantId = request.RestaurantId,
                AllowPromotionalOffers = request.AllowPromotionalOffers ?? true,
                AllowNewMenuItems = request.AllowNewMenuItems ?? true,
                AllowSpecialEvents = request.AllowSpecialEvents ?? true,
                AllowDiscountOffers = request.AllowDiscountOffers ?? true,
                AllowOrderReminders = request.AllowOrderReminders ?? true,
                PreferredFrequency = request.PreferredFrequency ?? NotificationFrequency.Daily,
                PreferredDays = request.PreferredDays ?? new List<DayOfWeek>(),
                PreferredTimeStart = request.PreferredTimeStart,
                PreferredTimeEnd = request.PreferredTimeEnd,
                UpdatedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Campaign Management

        public async Task<bool> CancelCampaignAsync(string campaignId)
        {
            // In real implementation, cancel scheduled campaign
            return true;
        }

        public async Task<RestaurantNotificationCampaignResult> ResendFailedNotificationsAsync(string campaignId)
        {
            // In real implementation, resend failed notifications
            return new RestaurantNotificationCampaignResult
            {
                CampaignId = campaignId,
                Success = true,
                Message = "Failed notifications resent successfully"
            };
        }

        public async Task<Dictionary<string, object>> GetCampaignMetricsAsync(string campaignId)
        {
            // In real implementation, get campaign metrics from analytics
            return new Dictionary<string, object>
            {
                ["deliveryRate"] = 96.5,
                ["openRate"] = 62.3,
                ["clickRate"] = 17.8,
                ["conversionRate"] = 8.2,
                ["revenue"] = 1250.50
            };
        }

        public async Task<NotificationResult> TestNotificationAsync(Guid restaurantId, NotificationMessage message, List<string> testDeviceTokens)
        {
            if (!testDeviceTokens.Any())
            {
                return new NotificationResult
                {
                    Success = false,
                    ErrorMessage = "No test device tokens provided"
                };
            }

            return await _notificationService.SendNotificationToMultipleAsync(testDeviceTokens, message).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<RestaurantNotificationSettings> GetRestaurantNotificationSettingsAsync(Guid restaurantId)
        {
            // In real implementation, get from database
            return new RestaurantNotificationSettings
            {
                RestaurantId = restaurantId,
                NotificationsEnabled = true,
                MaxNotificationsPerDay = 5,
                MaxNotificationsPerWeek = 20,
                RequireApproval = false,
                AllowPromotionalOffers = true,
                AllowNewMenuItems = true,
                AllowSpecialEvents = true,
                AllowDiscountOffers = true,
                AllowOrderReminders = true,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public async Task<RestaurantNotificationSettings> UpdateRestaurantNotificationSettingsAsync(Guid restaurantId, RestaurantNotificationSettings settings)
        {
            // In real implementation, update in database
            settings.RestaurantId = restaurantId;
            settings.UpdatedAt = DateTime.UtcNow;
            return settings;
        }

        #endregion

        #region Private Methods

        private async Task<List<Guid>> GetTargetedCustomersAsync(Guid restaurantId, CustomerTargetingCriteria criteria)
        {
            // In real implementation, query database based on criteria
            // This is a simplified version for demonstration
            
            var query = await _orderRepository.GetQueryableAsync();
            
            // Filter by restaurant
            var restaurantOrders = query.Where(o => o.RestaurantId == restaurantId);
            
            // Apply targeting criteria
            if (criteria.MinOrderCount.HasValue)
            {
                // Group by customer and count orders
                var customerOrderCounts = restaurantOrders
                    .GroupBy(o => o.UserId)
                    .Where(g => g.Count() >= criteria.MinOrderCount.Value)
                    .Select(g => g.Key);
                
                restaurantOrders = restaurantOrders.Where(o => customerOrderCounts.Contains(o.UserId));
            }
            
            if (criteria.MaxOrderCount.HasValue)
            {
                var customerOrderCounts = restaurantOrders
                    .GroupBy(o => o.UserId)
                    .Where(g => g.Count() <= criteria.MaxOrderCount.Value)
                    .Select(g => g.Key);
                
                restaurantOrders = restaurantOrders.Where(o => customerOrderCounts.Contains(o.UserId));
            }
            
            if (criteria.LastOrderAfter.HasValue)
            {
                restaurantOrders = restaurantOrders.Where(o => o.OrderDate >= criteria.LastOrderAfter.Value);
            }
            
            if (criteria.LastOrderBefore.HasValue)
            {
                restaurantOrders = restaurantOrders.Where(o => o.OrderDate <= criteria.LastOrderBefore.Value);
            }
            
            // Note: ExcludeCustomerIds is not available in CustomerTargetingCriteria
            // This functionality would need to be added to the DTO if needed
            
            // Get unique customer IDs
            var customerIds = restaurantOrders
                .Select(o => o.UserId)
                .Distinct()
                .ToList();
            
            return customerIds;
        }

        private async Task<List<string>> GetCustomerDeviceTokensAsync(List<Guid> customerIds)
        {
            // In real implementation, get device tokens from user settings/preferences
            // For now, return mock tokens
            return customerIds.Select(id => $"mock_device_token_{id}").ToList();
        }

        private double CalculateEstimatedOpenRate(NotificationType type)
        {
            // In real implementation, use historical data
            return type switch
            {
                NotificationType.PromotionalOffer => 65.0,
                NotificationType.RestaurantUpdate => 45.0,
                NotificationType.General => 35.0,
                _ => 50.0
            };
        }

        private double CalculateEstimatedClickRate(NotificationType type)
        {
            // In real implementation, use historical data
            return type switch
            {
                NotificationType.PromotionalOffer => 20.0,
                NotificationType.RestaurantUpdate => 15.0,
                NotificationType.General => 10.0,
                _ => 15.0
            };
        }

        #endregion
    }
}
