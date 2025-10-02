using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    /// <summary>
    /// API controller for restaurant notification management
    /// </summary>
    [Route("api/restaurant-notifications")]
    public class RestaurantNotificationController : AbpControllerBase, IRestaurantNotificationService
    {
        private readonly IRestaurantNotificationService _restaurantNotificationService;

        public RestaurantNotificationController(IRestaurantNotificationService restaurantNotificationService)
        {
            _restaurantNotificationService = restaurantNotificationService;
        }

        /// <summary>
        /// Send targeted notification to restaurant's previous customers
        /// </summary>
        [HttpPost("send-targeted")]
        public Task<RestaurantNotificationCampaignResult> SendTargetedNotificationAsync(RestaurantTargetedNotificationRequest request)
        {
            return _restaurantNotificationService.SendTargetedNotificationAsync(request);
        }

        /// <summary>
        /// Get restaurant's notification campaign history
        /// </summary>
        [HttpGet("campaigns/{restaurantId}")]
        public Task<List<RestaurantNotificationCampaignSummary>> GetRestaurantCampaignsAsync(Guid restaurantId, int skipCount = 0, int maxResultCount = 10)
        {
            return _restaurantNotificationService.GetRestaurantCampaignsAsync(restaurantId, skipCount, maxResultCount);
        }

        /// <summary>
        /// Get specific campaign details
        /// </summary>
        [HttpGet("campaigns/details/{campaignId}")]
        public Task<RestaurantNotificationCampaignResult> GetCampaignDetailsAsync(string campaignId)
        {
            return _restaurantNotificationService.GetCampaignDetailsAsync(campaignId);
        }

        /// <summary>
        /// Get restaurant notification analytics
        /// </summary>
        [HttpGet("analytics/{restaurantId}")]
        public Task<RestaurantNotificationAnalytics> GetRestaurantAnalyticsAsync(Guid restaurantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return _restaurantNotificationService.GetRestaurantAnalyticsAsync(restaurantId, startDate, endDate);
        }

        /// <summary>
        /// Get customer count for targeting criteria (preview)
        /// </summary>
        [HttpPost("preview-customers/{restaurantId}")]
        public Task<int> GetTargetedCustomerCountAsync(Guid restaurantId, CustomerTargetingCriteria criteria)
        {
            return _restaurantNotificationService.GetTargetedCustomerCountAsync(restaurantId, criteria);
        }

        /// <summary>
        /// Get customer segments for restaurant
        /// </summary>
        [HttpGet("customer-segments/{restaurantId}")]
        public Task<Dictionary<CustomerSegment, int>> GetCustomerSegmentsAsync(Guid restaurantId)
        {
            return _restaurantNotificationService.GetCustomerSegmentsAsync(restaurantId);
        }

        /// <summary>
        /// Create notification template for restaurant
        /// </summary>
        [HttpPost("templates")]
        public Task<RestaurantNotificationTemplate> CreateTemplateAsync(CreateRestaurantNotificationTemplateDto request)
        {
            return _restaurantNotificationService.CreateTemplateAsync(request);
        }

        /// <summary>
        /// Get restaurant notification templates
        /// </summary>
        [HttpGet("templates/{restaurantId}")]
        public Task<List<RestaurantNotificationTemplate>> GetRestaurantTemplatesAsync(Guid restaurantId)
        {
            return _restaurantNotificationService.GetRestaurantTemplatesAsync(restaurantId);
        }

        /// <summary>
        /// Update notification template
        /// </summary>
        [HttpPut("templates/{templateId}")]
        public Task<RestaurantNotificationTemplate> UpdateTemplateAsync(string templateId, CreateRestaurantNotificationTemplateDto request)
        {
            return _restaurantNotificationService.UpdateTemplateAsync(templateId, request);
        }

        /// <summary>
        /// Delete notification template
        /// </summary>
        [HttpDelete("templates/{templateId}")]
        public Task<bool> DeleteTemplateAsync(string templateId)
        {
            return _restaurantNotificationService.DeleteTemplateAsync(templateId);
        }

        /// <summary>
        /// Get customer notification preferences for restaurant
        /// </summary>
        [HttpGet("customer-preferences/{customerId}/{restaurantId}")]
        public Task<CustomerRestaurantNotificationPreferences> GetCustomerPreferencesAsync(Guid customerId, Guid restaurantId)
        {
            return _restaurantNotificationService.GetCustomerPreferencesAsync(customerId, restaurantId);
        }

        /// <summary>
        /// Update customer notification preferences for restaurant
        /// </summary>
        [HttpPut("customer-preferences/{customerId}")]
        public Task<CustomerRestaurantNotificationPreferences> UpdateCustomerPreferencesAsync(Guid customerId, UpdateCustomerRestaurantNotificationPreferencesDto request)
        {
            return _restaurantNotificationService.UpdateCustomerPreferencesAsync(customerId, request);
        }

        /// <summary>
        /// Cancel scheduled campaign
        /// </summary>
        [HttpPost("campaigns/{campaignId}/cancel")]
        public Task<bool> CancelCampaignAsync(string campaignId)
        {
            return _restaurantNotificationService.CancelCampaignAsync(campaignId);
        }

        /// <summary>
        /// Resend failed notifications for a campaign
        /// </summary>
        [HttpPost("campaigns/{campaignId}/resend-failed")]
        public Task<RestaurantNotificationCampaignResult> ResendFailedNotificationsAsync(string campaignId)
        {
            return _restaurantNotificationService.ResendFailedNotificationsAsync(campaignId);
        }

        /// <summary>
        /// Get campaign performance metrics
        /// </summary>
        [HttpGet("campaigns/{campaignId}/metrics")]
        public Task<Dictionary<string, object>> GetCampaignMetricsAsync(string campaignId)
        {
            return _restaurantNotificationService.GetCampaignMetricsAsync(campaignId);
        }

        /// <summary>
        /// Test notification with sample data
        /// </summary>
        [HttpPost("test/{restaurantId}")]
        public Task<NotificationResult> TestNotificationAsync(Guid restaurantId, NotificationMessage message, List<string> testDeviceTokens)
        {
            return _restaurantNotificationService.TestNotificationAsync(restaurantId, message, testDeviceTokens);
        }

        /// <summary>
        /// Get restaurant notification settings
        /// </summary>
        [HttpGet("settings/{restaurantId}")]
        public Task<RestaurantNotificationSettings> GetRestaurantNotificationSettingsAsync(Guid restaurantId)
        {
            return _restaurantNotificationService.GetRestaurantNotificationSettingsAsync(restaurantId);
        }

        /// <summary>
        /// Update restaurant notification settings
        /// </summary>
        [HttpPut("settings/{restaurantId}")]
        public Task<RestaurantNotificationSettings> UpdateRestaurantNotificationSettingsAsync(Guid restaurantId, RestaurantNotificationSettings settings)
        {
            return _restaurantNotificationService.UpdateRestaurantNotificationSettingsAsync(restaurantId, settings);
        }
    }

}
