using System;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// System settings DTO containing all application settings
    /// </summary>
    public class SystemSettingsDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public GeneralSettingsDto General { get; set; } = new();
        public NotificationSettingsDto Notifications { get; set; } = new();
        public DeliverySettingsDto Delivery { get; set; } = new();
        public SecuritySettingsDto Security { get; set; } = new();
        public MaintenanceSettingsDto Maintenance { get; set; } = new();
    }

    /// <summary>
    /// General application settings
    /// </summary>
    public class GeneralSettingsDto
    {
        public string AppName { get; set; } = "Delivery App";
        public string AppVersion { get; set; } = "1.3.0";
        public string DefaultLanguage { get; set; } = "ar";
        public string Timezone { get; set; } = "Asia/Riyadh";
        public string Currency { get; set; } = "SYP";
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string FaviconUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Notification settings
    /// </summary>
    public class NotificationSettingsDto
    {
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool OrderUpdates { get; set; } = true;
        public bool SystemAlerts { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;
        public bool WeeklyReports { get; set; } = true;
        public bool MonthlyReports { get; set; } = true;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool SmtpUseSsl { get; set; } = true;
        public string SmsProvider { get; set; } = string.Empty;
        public string SmsApiKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Delivery settings
    /// </summary>
    public class DeliverySettingsDto
    {
        public double DefaultDeliveryRadius { get; set; } = 10.0;
        public int MaxDeliveryTime { get; set; } = 60;
        public double DeliveryFee { get; set; } = 5.0;
        public double FreeDeliveryThreshold { get; set; } = 50.0;
        public double MaxDeliveryDistance { get; set; } = 25.0;
        public double RushDeliveryFee { get; set; } = 10.0;
        public int RushDeliveryTime { get; set; } = 30;
        public bool AllowScheduledDelivery { get; set; } = true;
        public int MaxScheduledDays { get; set; } = 7;
        public double MinimumOrderAmount { get; set; } = 20.0;
        public bool RequireDeliveryConfirmation { get; set; } = true;
        
        // New city-based delivery settings
        public double InTownDistanceThreshold { get; set; } = 5.0; // km - within this distance is considered in-town
        public double OutOfTownRatePerKm { get; set; } = 2.0; // Rate per km for out-of-town delivery
        public double InTownBaseFee { get; set; } = 5.0; // Fixed fee for in-town delivery
        public double OutOfTownBaseFee { get; set; } = 8.0; // Base fee for out-of-town delivery
    }

    /// <summary>
    /// Security settings
    /// </summary>
    public class SecuritySettingsDto
    {
        public int SessionTimeout { get; set; } = 30;
        public bool RequireStrongPasswords { get; set; } = true;
        public bool EnableTwoFactor { get; set; } = false;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDuration { get; set; } = 15;
        public bool RequireEmailVerification { get; set; } = true;
        public bool RequirePhoneVerification { get; set; } = false;
        public bool EnableApiRateLimiting { get; set; } = true;
        public int ApiRateLimit { get; set; } = 100;
        public bool EnableAuditLogging { get; set; } = true;
        public bool RequireHttps { get; set; } = true;
    }

    /// <summary>
    /// Maintenance settings
    /// </summary>
    public class MaintenanceSettingsDto
    {
        public bool MaintenanceMode { get; set; } = false;
        public string MaintenanceMessage { get; set; } = "System is under maintenance. Please try again later.";
        public string BackupFrequency { get; set; } = "daily";
        public int LogRetentionDays { get; set; } = 30;
        public bool EnablePerformanceMonitoring { get; set; } = true;
        public int DatabaseCleanupInterval { get; set; } = 7;
        public bool EnableErrorReporting { get; set; } = true;
        public string ErrorReportingEmail { get; set; } = string.Empty;
        public bool EnableSystemHealthChecks { get; set; } = true;
        public int HealthCheckInterval { get; set; } = 5;
    }
}
