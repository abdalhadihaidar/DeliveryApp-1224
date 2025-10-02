using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Microsoft.AspNetCore.SignalR;
using DeliveryApp.Hubs;
using Microsoft.Extensions.DependencyInjection;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Services;

namespace DeliveryApp;

[DependsOn(
    typeof(DeliveryAppDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(DeliveryAppApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class DeliveryAppApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<DeliveryAppApplicationModule>();
        });

        // Configure AuthSettings from JwtSettings configuration
        context.Services.Configure<AuthSettings>(options =>
        {
            var configuration = context.Services.GetConfiguration();
            var jwtSettings = configuration.GetSection("JwtSettings");
            
            options.JwtSecretKey = jwtSettings["Key"] ?? "vVUX5TuRkQPpM5YVvKwt5zvmywJ7tnN8";
            options.JwtIssuer = jwtSettings["Issuer"] ?? "DeliveryApp";
            options.JwtAudience = jwtSettings["Audience"] ?? "DeliveryApp";
            options.AccessTokenExpiryMinutes = 60;
            options.RefreshTokenExpiryDays = 30;
            options.VerificationCodeExpiryMinutes = 15;
            options.RequireEmailVerification = true;
            options.RequirePhoneVerification = false;
            options.AllowEmailLogin = true;
            options.AllowPhoneLogin = true;
            options.MaxLoginAttempts = 5;
            options.LockoutDurationMinutes = 30;
        });

        // Configure JWT Settings for Mobile Auth
        context.Services.Configure<JwtSettings>(options =>
        {
            var configuration = context.Services.GetConfiguration();
            var jwtSettings = configuration.GetSection("JwtSettings");
            
            options.SecretKey = jwtSettings["Key"] ?? "your-super-secret-key-with-at-least-32-characters";
            options.Issuer = jwtSettings["Issuer"] ?? "DeliveryApp";
            options.Audience = jwtSettings["Audience"] ?? "DeliveryAppMobile";
        });

        // Configure SignalR
        context.Services.AddSignalR();
        
        // Register application services
        context.Services.AddTransient<DeliveryApp.Application.Contracts.Services.IDashboardAppService, DeliveryApp.Application.Services.DashboardAppService>();
        context.Services.AddTransient<IFirebaseNotificationService, FirebaseNotificationService>();
        context.Services.AddTransient<INotificationAnalyticsService, NotificationAnalyticsService>();
        context.Services.AddTransient<IMobileAuthService, MobileAuthService>();
        context.Services.AddTransient<ISettingsAppService, SettingsAppService>();
        context.Services.AddTransient<IRestaurantOwnerNotificationService, RestaurantOwnerNotificationService>();
    }
}
