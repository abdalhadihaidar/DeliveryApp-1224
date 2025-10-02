using System;
using System.Text.Json;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Application service for managing system settings
    /// </summary>
    [RemoteService]
    public class SettingsAppService : ApplicationService, ISettingsAppService, ITransientDependency
    {
        private readonly ILogger<SettingsAppService> _logger;
        private static SystemSettingsDto? _cachedSettings;

        public SettingsAppService(ILogger<SettingsAppService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get all system settings
        /// </summary>
        /// <returns>System settings DTO</returns>
        public async Task<SystemSettingsDto> GetSettingsAsync()
        {
            try
            {
                // Return cached settings if available, otherwise return defaults
                if (_cachedSettings != null)
                {
                    return _cachedSettings;
                }

                // Return default settings
                var defaultSettings = GetDefaultSettings();
                _cachedSettings = defaultSettings;
                
                _logger.LogInformation("Retrieved system settings");
                return await Task.FromResult(defaultSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system settings");
                throw;
            }
        }

        /// <summary>
        /// Update system settings
        /// </summary>
        /// <param name="settings">Settings to update</param>
        /// <returns>Updated settings</returns>
        public async Task<SystemSettingsDto> UpdateSettingsAsync(SystemSettingsDto settings)
        {
            try
            {
                // Validate settings
                if (settings == null)
                {
                    throw new ArgumentNullException(nameof(settings));
                }

                // Update timestamp
                settings.UpdatedAt = DateTime.UtcNow;
                if (string.IsNullOrEmpty(settings.Id))
                {
                    settings.Id = Guid.NewGuid().ToString();
                    settings.CreatedAt = DateTime.UtcNow;
                }

                // Cache the updated settings
                _cachedSettings = settings;

                _logger.LogInformation("Updated system settings");
                return await Task.FromResult(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system settings");
                throw;
            }
        }

        /// <summary>
        /// Reset settings to default values
        /// </summary>
        /// <returns>Default settings</returns>
        public async Task<SystemSettingsDto> ResetToDefaultsAsync()
        {
            try
            {
                var defaultSettings = GetDefaultSettings();
                _cachedSettings = defaultSettings;

                _logger.LogInformation("Reset system settings to defaults");
                return await Task.FromResult(defaultSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting system settings to defaults");
                throw;
            }
        }

        /// <summary>
        /// Export settings as JSON string
        /// </summary>
        /// <returns>Settings as JSON string</returns>
        public async Task<string> ExportSettingsAsync()
        {
            try
            {
                var settings = await GetSettingsAsync();
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Exported system settings");
                return json;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting system settings");
                throw;
            }
        }

        /// <summary>
        /// Import settings from JSON string
        /// </summary>
        /// <param name="settingsJson">Settings JSON string</param>
        /// <returns>Imported settings</returns>
        public async Task<SystemSettingsDto> ImportSettingsAsync(string settingsJson)
        {
            try
            {
                if (string.IsNullOrEmpty(settingsJson))
                {
                    throw new ArgumentException("Settings JSON cannot be null or empty", nameof(settingsJson));
                }

                var settings = JsonSerializer.Deserialize<SystemSettingsDto>(settingsJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (settings == null)
                {
                    throw new InvalidOperationException("Failed to deserialize settings JSON");
                }

                // Update the settings
                var updatedSettings = await UpdateSettingsAsync(settings);

                _logger.LogInformation("Imported system settings");
                return updatedSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing system settings");
                throw;
            }
        }

        /// <summary>
        /// Get specific settings category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>Category settings</returns>
        public async Task<object> GetSettingsCategoryAsync(string category)
        {
            try
            {
                var settings = await GetSettingsAsync();

                return category.ToLowerInvariant() switch
                {
                    "general" => settings.General,
                    "notifications" => settings.Notifications,
                    "delivery" => settings.Delivery,
                    "security" => settings.Security,
                    "maintenance" => settings.Maintenance,
                    _ => throw new ArgumentException($"Unknown settings category: {category}", nameof(category))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving settings category: {Category}", category);
                throw;
            }
        }

        /// <summary>
        /// Update specific settings category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="categorySettings">Category settings to update</param>
        /// <returns>Updated system settings</returns>
        public async Task<SystemSettingsDto> UpdateSettingsCategoryAsync(string category, object categorySettings)
        {
            try
            {
                var settings = await GetSettingsAsync();

                switch (category.ToLowerInvariant())
                {
                    case "general":
                        if (categorySettings is GeneralSettingsDto generalSettings)
                        {
                            settings.General = generalSettings;
                        }
                        break;
                    case "notifications":
                        if (categorySettings is NotificationSettingsDto notificationSettings)
                        {
                            settings.Notifications = notificationSettings;
                        }
                        break;
                    case "delivery":
                        if (categorySettings is DeliverySettingsDto deliverySettings)
                        {
                            settings.Delivery = deliverySettings;
                        }
                        break;
                    case "security":
                        if (categorySettings is SecuritySettingsDto securitySettings)
                        {
                            settings.Security = securitySettings;
                        }
                        break;
                    case "maintenance":
                        if (categorySettings is MaintenanceSettingsDto maintenanceSettings)
                        {
                            settings.Maintenance = maintenanceSettings;
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unknown settings category: {category}", nameof(category));
                }

                var updatedSettings = await UpdateSettingsAsync(settings);

                _logger.LogInformation("Updated settings category: {Category}", category);
                return updatedSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings category: {Category}", category);
                throw;
            }
        }

        /// <summary>
        /// Get default system settings
        /// </summary>
        /// <returns>Default settings</returns>
        private static SystemSettingsDto GetDefaultSettings()
        {
            return new SystemSettingsDto
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                General = new GeneralSettingsDto
                {
                    AppName = "Delivery App",
                    AppVersion = "1.3.0",
                    DefaultLanguage = "ar",
                    Timezone = "Asia/Riyadh",
                    Currency = "SYP",
                    CompanyName = "Delivery App Company",
                    CompanyEmail = "info@deliveryapp.com",
                    CompanyPhone = "+963-11-123-4567",
                    CompanyAddress = "Damascus, Syria",
                    LogoUrl = "/assets/images/logo.png",
                    FaviconUrl = "/assets/images/favicon.ico"
                },
                Notifications = new NotificationSettingsDto
                {
                    EmailNotifications = true,
                    SmsNotifications = true,
                    PushNotifications = true,
                    OrderUpdates = true,
                    SystemAlerts = true,
                    MarketingEmails = false,
                    WeeklyReports = true,
                    MonthlyReports = true,
                    SmtpServer = "smtp.gmail.com",
                    SmtpPort = 587,
                    SmtpUsername = "",
                    SmtpPassword = "",
                    SmtpUseSsl = true,
                    SmsProvider = "Twilio",
                    SmsApiKey = ""
                },
                Delivery = new DeliverySettingsDto
                {
                    DefaultDeliveryRadius = 10.0,
                    MaxDeliveryTime = 60,
                    DeliveryFee = 5.0,
                    FreeDeliveryThreshold = 50.0,
                    MaxDeliveryDistance = 25.0,
                    RushDeliveryFee = 10.0,
                    RushDeliveryTime = 30,
                    AllowScheduledDelivery = true,
                    MaxScheduledDays = 7,
                    MinimumOrderAmount = 20.0,
                    RequireDeliveryConfirmation = true
                },
                Security = new SecuritySettingsDto
                {
                    SessionTimeout = 30,
                    RequireStrongPasswords = true,
                    EnableTwoFactor = false,
                    MaxLoginAttempts = 5,
                    LockoutDuration = 15,
                    RequireEmailVerification = true,
                    RequirePhoneVerification = false,
                    EnableApiRateLimiting = true,
                    ApiRateLimit = 100,
                    EnableAuditLogging = true,
                    RequireHttps = true
                },
                Maintenance = new MaintenanceSettingsDto
                {
                    MaintenanceMode = false,
                    MaintenanceMessage = "System is under maintenance. Please try again later.",
                    BackupFrequency = "daily",
                    LogRetentionDays = 30,
                    EnablePerformanceMonitoring = true,
                    DatabaseCleanupInterval = 7,
                    EnableErrorReporting = true,
                    ErrorReportingEmail = "admin@deliveryapp.com",
                    EnableSystemHealthChecks = true,
                    HealthCheckInterval = 5
                }
            };
        }
    }
}
