using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Contract for the user-engagement/business-intelligence service consumed by application layer.
    /// Only contains members currently referenced by the implementation.
    /// </summary>
    [RemoteService]
    public interface IUserEngagementService : IApplicationService
    {
        Task<NotificationPreference> GetNotificationPreferencesAsync(string userId);
        Task<bool> UpdateNotificationPreferencesAsync(string userId, UpdateNotificationPreferencesDto dto);
        Task<UserEngagementMetrics> GetUserEngagementMetricsAsync(string userId, EngagementMetricsFilter filter);
        Task<bool> TrackUserActivityAsync(string userId, string activityType, Dictionary<string, object> metadata = null);
        Task<List<EngagementCampaign>> GetActiveEngagementCampaignsAsync(string userId);
        Task<bool> CreateEngagementCampaignAsync(CreateEngagementCampaignDto dto);
        Task<EngagementAnalyticsDto> GetEngagementAnalyticsAsync(EngagementAnalyticsFilter filter);
        Task<bool> ScheduleNotificationAsync(ScheduleNotificationDto dto);
        Task<PersonalizationProfile> GetPersonalizationProfileAsync(string userId);
    }
} 
