using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeliveryApp.EntityFrameworkCore;
using DeliveryApp.Localization;
using DeliveryApp.MultiTenancy;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using DeliveryApp.Application.Services;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.OpenIddict;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI;
using DeliveryApp.Web.HostedServices;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.Identity.Web;
using Volo.Abp.UI.Navigation;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.SettingManagement.Web;
using DeliveryApp.Web.Menus;
using DeliveryApp.Web.HostedServices;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using System.Security.Claims;
using Serilog;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DeliveryApp.Web;

[DependsOn(
typeof(DeliveryAppHttpApiModule),
    typeof(DeliveryAppApplicationModule),
    typeof(DeliveryAppEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpSettingManagementWebModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
    )]
public class DeliveryAppWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(DeliveryAppResource),
                typeof(DeliveryAppDomainModule).Assembly,
                typeof(DeliveryAppDomainSharedModule).Assembly,
                typeof(DeliveryAppApplicationModule).Assembly,
                typeof(DeliveryAppApplicationContractsModule).Assembly,
                typeof(DeliveryAppWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddServer(options =>
            {
                options
                    .SetTokenEndpointUris("/connect/token")
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetUserInfoEndpointUris("/connect/userinfo");

                // Add ephemeral signing key for OpenIddict (required for asymmetric operations)
                options.AddEphemeralSigningKey();
                
                // Use symmetric key for encryption (256-bit key for OpenIddict)
                var jwtSecret = configuration["JwtSettings:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP";
                
                // Debug: Print JWT secret info
                Console.WriteLine($"=== OPENIDDICT WEB MODULE DEBUG ===");
                Console.WriteLine($"JWT Secret from config: {configuration["JwtSettings:SecretKey"]?.Substring(0, Math.Min(10, configuration["JwtSettings:SecretKey"]?.Length ?? 0))}...");
                Console.WriteLine($"JWT Secret from env: {Environment.GetEnvironmentVariable("JWT_SECRET_KEY")?.Substring(0, Math.Min(10, Environment.GetEnvironmentVariable("JWT_SECRET_KEY")?.Length ?? 0))}...");
                Console.WriteLine($"JWT Secret final: {jwtSecret.Substring(0, Math.Min(10, jwtSecret.Length))}...");
                
                // OpenIddict requires exactly 256 bits (32 characters) for encryption key
                var encryptionKey = jwtSecret.Length >= 32 ? jwtSecret.Substring(0, 32) : jwtSecret.PadRight(32, '0');
                var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(encryptionKey));
                options.AddEncryptionKey(key);

                // Set the issuer
                var issuer = configuration["JwtSettings:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "http://wasel.somee.com/";
                Console.WriteLine($"JWT Issuer: {issuer}");
                options.SetIssuer(issuer);
                Console.WriteLine($"=== END OPENIDDICT WEB MODULE DEBUG ===");

                // Register scopes
                options.RegisterScopes("DeliveryApp", "offline_access");

                // Register grant types
                options.AllowPasswordFlow()
                       .AllowRefreshTokenFlow();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options
                options.UseAspNetCore()
                       // Disabled TokenEndpointPassthrough to avoid ABP TokenController NRE
                       // .EnableTokenEndpointPassthrough()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough()
                       .DisableTransportSecurityRequirement(); // Allow HTTP in development
            });

            builder.AddValidation(options =>
            {
                options.AddAudiences("DeliveryApp", "DeliveryAppMobile");
                options.UseLocalServer();
                options.UseAspNetCore();
                
                // Configure validation to work with OpenIddict tokens
                var validationIssuer = configuration["JwtSettings:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "http://wasel.somee.com/";
                options.SetIssuer(validationIssuer);
            });



        });

        PreConfigure<OpenIddictServerAspNetCoreOptions>(options =>
        {
            // Removed: options.EnableTokenEndpointPassthrough = false;
        });

        // Use symmetric keys for both development and production
        // This avoids the need for certificate files and works well for this application
        PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
        {
            options.AddDevelopmentEncryptionAndSigningCertificate = false;
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Removed manual Identity registration. ABP handles Identity setup.

        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();
        
        // Ensure connection string is properly configured
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Warning("Connection string 'Default' not found in configuration. Using environment variable fallback.");
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        }
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            Log.Information("Connection string configured successfully");
        }
        else
        {
            Log.Error("No connection string found in configuration or environment variables");
        }

        // Add CORS for mobile app and dashboard compatibility
        context.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
            
            options.AddPolicy("AllowDashboard", builder =>
            {
                builder.WithOrigins(
                        "http://localhost:4200", 
                        "https://localhost:4200", 
                        "http://dashboard.waselsy.com", 
                        "https://dashboard.waselsy.com",
                        "http://localhost:3000",
                        "https://localhost:3000",
                        "http://localhost:8080",
                        "https://localhost:8080"
                    )
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials()
                       .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight for 24 hours
            });
            
            // CORS policy for SignalR with credentials - Allow all origins
            options.AddPolicy("AllowSignalR", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
            
            // Default CORS policy for API endpoints
            options.AddDefaultPolicy(builder =>
            {
                var allowedOrigins = new List<string>
                {
                    "http://localhost:4200", 
                    "https://localhost:4200", 
                    "http://localhost:3000",
                    "https://localhost:3000",
                    "http://localhost:8080",
                    "https://localhost:8080"
                };

                // Add configured origins from environment variables
                var dashboardUrl = configuration["App:DashboardUrl"] ?? Environment.GetEnvironmentVariable("DASHBOARD_URL");
                if (!string.IsNullOrEmpty(dashboardUrl))
                {
                    allowedOrigins.Add(dashboardUrl);
                    allowedOrigins.Add(dashboardUrl.Replace("http://", "https://"));
                }

                // Add wildcard domains if configured
                var wildcardDomain = configuration["App:WildcardDomain"] ?? Environment.GetEnvironmentVariable("WILDCARD_DOMAIN");
                if (!string.IsNullOrEmpty(wildcardDomain))
                {
                    allowedOrigins.Add($"https://*.{wildcardDomain}");
                }

                builder.WithOrigins(allowedOrigins.ToArray())
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        // Add SignalR
        context.Services.AddSignalR();

        // Register notification services
        context.Services.AddTransient<DeliveryApp.Application.Contracts.Services.IRealtimeNotifier, DeliveryApp.Application.Services.SignalRNotifier>();
        context.Services.AddTransient<DeliveryApp.Application.Contracts.Services.IEmailNotifier, DeliveryApp.Application.Services.SendPulseEmailNotifier>();
        context.Services.AddHttpClient<DeliveryApp.Application.Services.SendPulseEmailNotifier>();

        // Configure OpenIddict token lifetimes
        context.Services.Configure<OpenIddictServerOptions>(options =>
        {
            options.AccessTokenLifetime = TimeSpan.FromHours(24);
            options.RefreshTokenLifetime = TimeSpan.FromDays(30);
        });

        // Force disable OpenIddict token endpoint passthrough
        Configure<OpenIddictServerAspNetCoreOptions>(options =>
        {
            // Enable token endpoint passthrough for custom controller handling
            options.EnableTokenEndpointPassthrough = true;
        });

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);
        ConfigureSignalR(context.Services);
        ConfigureNotifications(context.Services);
        
        // Disable ABP libs check for Docker/cloud deployments
        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });
        
        // Disable ABP authorization by registering a custom permission checker that always allows access
        context.Services.AddSingleton<IPermissionChecker, AlwaysAllowPermissionChecker>();

        // Register HttpClient factory so services like EmailService can receive HttpClient via DI
        context.Services.AddHttpClient();

        // Configure antiforgery to ignore API endpoints
        context.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        {
            options.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
        });

        // Configure memory optimization settings
        context.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB limit
            options.Limits.MaxConcurrentConnections = 100;
            options.Limits.MaxConcurrentUpgradedConnections = 100;
        });

        // Configure garbage collection for better memory management
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        // Database migrations should only run once, not on every startup
        // context.Services.AddHostedService<WebDbMigrationHostedService>();
    }

    private void ConfigureSignalR(IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
        
        // Configure JWT authentication for SignalR
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
    }

    private void ConfigureNotifications(IServiceCollection services)
    {
        // Register notification services
        services.AddTransient<DeliveryApp.Application.Contracts.Services.IRealtimeNotifier, DeliveryApp.Application.Services.SignalRNotifier>();
        services.AddTransient<DeliveryApp.Application.Contracts.Services.IEmailNotifier, DeliveryApp.Application.Services.SendPulseEmailNotifier>();
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        // Use OpenIddict validation as the single source of truth for authentication
        context.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });

        context.Services.AddTransient<IAbpClaimsPrincipalContributor, DeliveryAppClaimsPrincipalContributor>();
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            // Add our custom global styles to the existing bundle
            options.StyleBundles.Configure(
                BasicThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<DeliveryAppWebModule>();
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<DeliveryAppDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DeliveryApp.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<DeliveryAppDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DeliveryApp.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<DeliveryAppApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DeliveryApp.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<DeliveryAppApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DeliveryApp.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<DeliveryAppWebModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new DeliveryAppMenuContributor());
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers but exclude services that have manual controllers
            options.ConventionalControllers.Create(typeof(DeliveryAppApplicationModule).Assembly, opts =>
            {
                opts.TypePredicate = type => 
                    type != typeof(DeliveryApp.Application.Services.RestaurantOwnerAppService) &&
                    type != typeof(DeliveryApp.Application.Services.DashboardAppService) &&
                    type != typeof(DeliveryApp.Application.Services.UserAppService) &&
                    type != typeof(DeliveryApp.Application.Services.CustomerAppService) &&
                    type != typeof(DeliveryApp.Application.Services.DeliveryPersonAppService) &&
                    type != typeof(DeliveryApp.Application.Services.AuthService) &&
                    type != typeof(DeliveryApp.Application.Services.RestaurantCategoryAppService) &&
                    type != typeof(DeliveryApp.Application.Services.MealCategoryAppService) &&
                    type != typeof(DeliveryApp.Application.Services.StripePaymentService) &&
                    type != typeof(DeliveryApp.Application.Services.FinancialManagementService);
            });
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Waseel API", Version = "v1", Description = "وصيل • REST API" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
                
                // Ensure Swagger is always available in all environments
                options.IgnoreObsoleteActions();
                options.IgnoreObsoleteProperties();

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri("/connect/token", UriKind.Relative),
                            Scopes = new Dictionary<string, string> { { "DeliveryApp", "DeliveryApp API" } }
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[] { "DeliveryApp" }
                    }
                });

                options.OperationFilter<LanguageHeaderOperationFilter>();
            });
    }

    public class LanguageHeaderOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
    {
        public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("en")
                }
            });
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseAbpRequestLocalization();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Disable HTTPS redirection for API endpoints to prevent issues
        // app.UseHttpsRedirection();
        app.UseCorrelationId();
        
        // Configure static files with proper MIME types
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                var path = ctx.File.Name.ToLowerInvariant();
                if (path.EndsWith(".css"))
                {
                    ctx.Context.Response.Headers.Append("Content-Type", "text/css");
                }
                else if (path.EndsWith(".js"))
                {
                    ctx.Context.Response.Headers.Append("Content-Type", "application/javascript");
                }
            }
        });
        
        app.MapAbpStaticAssets();
        app.UseRouting();
        // Enable CORS so that the browser can successfully perform
        // cross-origin requests coming from the Flutter Web dev server (e.g. http://localhost:61073).
        // This must be placed between UseRouting and UseAuthentication/UseAuthorization.
        app.UseCors("AllowDashboard");
        app.UseAuthentication();
        app.UseAuthorization();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAntiforgery();

        // Always enable Swagger in all environments
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Waseel API");
            options.DocumentTitle = "وصيل • Waseel API Docs";
            options.HeadContent += "<link rel=\"icon\" type=\"image/png\" href=\"/swagger-ui/favicon-32x32.png\" />";
            options.RoutePrefix = "swagger"; // Ensure Swagger UI is accessible at /swagger

            // Add mobile app download link to Swagger UI
            options.HeadContent += @"
                <style>
                    .swagger-ui .topbar { 
                        background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
                        padding: 10px 0;
                    }
                    .mobile-app-link {
                        position: absolute;
                        right: 20px;
                        top: 50%;
                        transform: translateY(-50%);
                        background: rgba(255,255,255,0.2);
                        color: white;
                        padding: 8px 16px;
                        border-radius: 20px;
                        text-decoration: none;
                        font-size: 14px;
                        transition: all 0.3s ease;
                    }
                    .mobile-app-link:hover {
                        background: rgba(255,255,255,0.3);
                        color: white;
                        text-decoration: none;
                    }
                </style>";
             
        });

        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapHub<DeliveryApp.Application.Hubs.UserApprovalHub>("/signalr/userApprovalHub");
            endpoints.MapHub<DeliveryApp.Web.Hubs.ChatHub>("/hubs/chat");
        });
    }
}
