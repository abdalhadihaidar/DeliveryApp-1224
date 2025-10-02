using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Application service for managing system settings
    /// </summary>
    public interface ISettingsAppService : IApplicationService
    {
        /// <summary>
        /// Get all system settings
        /// </summary>
        /// <returns>System settings DTO</returns>
        Task<SystemSettingsDto> GetSettingsAsync();

        /// <summary>
        /// Update system settings
        /// </summary>
        /// <param name="settings">Settings to update</param>
        /// <returns>Updated settings</returns>
        Task<SystemSettingsDto> UpdateSettingsAsync(SystemSettingsDto settings);

        /// <summary>
        /// Reset settings to default values
        /// </summary>
        /// <returns>Default settings</returns>
        Task<SystemSettingsDto> ResetToDefaultsAsync();

        /// <summary>
        /// Export settings as JSON string
        /// </summary>
        /// <returns>Settings as JSON string</returns>
        Task<string> ExportSettingsAsync();

        /// <summary>
        /// Import settings from JSON string
        /// </summary>
        /// <param name="settingsJson">Settings JSON string</param>
        /// <returns>Imported settings</returns>
        Task<SystemSettingsDto> ImportSettingsAsync(string settingsJson);

        /// <summary>
        /// Get specific settings category
        /// </summary>
        /// <param name="category">Category name (general, notifications, delivery, security, maintenance)</param>
        /// <returns>Category settings</returns>
        Task<object> GetSettingsCategoryAsync(string category);

        /// <summary>
        /// Update specific settings category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="categorySettings">Category settings to update</param>
        /// <returns>Updated system settings</returns>
        Task<SystemSettingsDto> UpdateSettingsCategoryAsync(string category, object categorySettings);
    }
}
