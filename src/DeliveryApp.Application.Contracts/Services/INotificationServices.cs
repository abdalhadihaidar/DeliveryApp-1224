using System.Threading.Tasks;
using System.Collections.Generic;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{

    /// <summary>
    /// Interface for notification template service
    /// </summary>
    public interface INotificationTemplateService
    {
        /// <summary>
        /// Create notification template
        /// </summary>
        Task<NotificationTemplate> CreateTemplateAsync(CreateNotificationTemplateDto template);

        /// <summary>
        /// Update notification template
        /// </summary>
        Task<NotificationTemplate> UpdateTemplateAsync(string templateId, UpdateNotificationTemplateDto template);

        /// <summary>
        /// Get notification template by ID
        /// </summary>
        Task<NotificationTemplate> GetTemplateAsync(string templateId);

        /// <summary>
        /// Get all notification templates
        /// </summary>
        Task<List<NotificationTemplate>> GetAllTemplatesAsync();

        /// <summary>
        /// Delete notification template
        /// </summary>
        Task<bool> DeleteTemplateAsync(string templateId);

        /// <summary>
        /// Render notification from template
        /// </summary>
        Task<NotificationMessage> RenderNotificationAsync(string templateId, Dictionary<string, object> parameters);

        /// <summary>
        /// Get template usage statistics
        /// </summary>
        Task<TemplateStatistics> GetTemplateStatisticsAsync(string templateId);
    }

    /// <summary>
    /// Interface for notification preferences service
    /// </summary>
    public interface INotificationPreferencesService
    {
        /// <summary>
        /// Get user notification preferences
        /// </summary>
        Task<NotificationPreferences> GetUserPreferencesAsync(string userId);

        /// <summary>
        /// Update user notification preferences
        /// </summary>
        Task<NotificationPreferences> UpdateUserPreferencesAsync(string userId, UpdateNotificationPreferencesDto preferences);

        /// <summary>
        /// Check if user should receive notification
        /// </summary>
        Task<bool> ShouldReceiveNotificationAsync(string userId, NotificationType notificationType);

        /// <summary>
        /// Get notification settings for user
        /// </summary>
        Task<NotificationSettings> GetNotificationSettingsAsync(string userId);

        /// <summary>
        /// Update notification settings
        /// </summary>
        Task<NotificationSettings> UpdateNotificationSettingsAsync(string userId, NotificationSettings settings);

        /// <summary>
        /// Get default notification preferences
        /// </summary>
        Task<NotificationPreferences> GetDefaultPreferencesAsync();

        /// <summary>
        /// Bulk update notification preferences
        /// </summary>
        Task<bool> BulkUpdatePreferencesAsync(List<UserNotificationPreference> preferences);
    }

}

