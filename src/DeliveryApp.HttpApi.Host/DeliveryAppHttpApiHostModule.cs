using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using DeliveryApp.EntityFrameworkCore;
using DeliveryApp.Localization;
using DeliveryApp.MultiTenancy;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using DeliveryApp.Application.Services;
using DeliveryApp.Application.Configuration;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.OpenIddict;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerEvents;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.Identity;
using Volo.Abp.Account;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;

namespace DeliveryApp.HttpApi.Host;

[DependsOn(
    typeof(DeliveryAppHttpApiModule),
    typeof(DeliveryAppApplicationModule),
    typeof(DeliveryAppEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpAccountHttpApiModule)
    )]
public class DeliveryAppHttpApiHostModule : AbpModule
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
                typeof(DeliveryAppHttpApiHostModule).Assembly
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

                // Register scopes
                options.RegisterScopes("DeliveryApp", "offline_access");

                // Register grant types
                options.AllowPasswordFlow()
                       .AllowRefreshTokenFlow();

                // Add custom password flow handler to bypass ABP TokenController issues
                options.AddEventHandler<HandleTokenRequestContext>(builder =>
                {
                    builder.UseSingletonHandler<DeliveryApp.HttpApi.Host.Handlers.PasswordFlowHandler>();
                });

                // Set the issuer
                var issuer = configuration["JwtSettings:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "http://wasel.somee.com/";
                options.SetIssuer(issuer);

                
                // Configure encryption and signing certificates
                if (hostingEnvironment.IsDevelopment())
                {
                    options.AddDevelopmentSigningCertificate()
                           .AddDevelopmentEncryptionCertificate();
                }
                else
                {
                    // For production, use symmetric keys for better compatibility with shared hosting
                    // Add ephemeral signing key for OpenIddict (required for asymmetric operations)
                    options.AddEphemeralSigningKey();
                    
                    // Use symmetric key for encryption (256-bit key for OpenIddict)
                    var jwtSecret = configuration["JwtSettings:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "YXJzdRf2yF8bjY4iIvNc8fn6VQSR5nwGWLZfkvsErfKVEOI5hu6tcyh8uvfjjUmP";
                    // OpenIddict requires exactly 256 bits (32 characters) for encryption key
                    var encryptionKey = jwtSecret.Length >= 32 ? jwtSecret.Substring(0, 32) : jwtSecret.PadRight(32, '0');
                    var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(encryptionKey));
                    options.AddEncryptionKey(key);
                }

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options
                options.UseAspNetCore()
                       // Disabled to avoid ABP TokenController ServiceScopeFactory NRE
                       // .EnableTokenEndpointPassthrough()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough()
                       .DisableTransportSecurityRequirement(); // For HTTP in development
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

        PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
        {
            options.AddDevelopmentEncryptionAndSigningCertificate = hostingEnvironment.IsDevelopment();
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Configure ABP Framework to handle missing tables gracefully
        Configure<Volo.Abp.BackgroundJobs.AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false; // Disable background job execution
        });

        // Configure Entity Framework to handle database errors gracefully
        context.Services.Configure<Microsoft.EntityFrameworkCore.DbContextOptionsBuilder>(options =>
        {
            options.EnableSensitiveDataLogging(hostingEnvironment.IsDevelopment());
            options.EnableDetailedErrors(hostingEnvironment.IsDevelopment());
        });

        // Add global exception handling for database errors
        context.Services.AddScoped<DeliveryApp.HttpApi.Host.Middleware.DatabaseExceptionMiddleware>();

        // Configure data protection for production IIS environment
        if (!hostingEnvironment.IsDevelopment())
        {
            var dataProtectionPath = Path.Combine(hostingEnvironment.ContentRootPath, "DataProtection-Keys");
            context.Services.AddDataProtection()
                .SetApplicationName("DeliveryApp")
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
        }

        // Add CORS for mobile app compatibility
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
                       .AllowCredentials()
                       .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
            });
        });

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);
        context.Services.AddSingleton<IPermissionChecker, AlwaysAllowPermissionChecker>();
        

        // Register HttpClient factory so services like EmailService can receive HttpClient via DI
        context.Services.AddHttpClient();
        
        // Register HttpClient for AuthService and MobileAuthService specifically
        context.Services.AddHttpClient<DeliveryApp.Application.Services.AuthService>();
        context.Services.AddHttpClient<DeliveryApp.Application.Services.MobileAuthService>();
        context.Services.AddHttpClient<DeliveryApp.Application.Services.SendPulseEmailNotifier>();

        // Register a background hosted service that ensures database schema is migrated
        // and seed data is inserted when the HTTP API host starts up.
        context.Services.AddHostedService<DeliveryApp.HttpApi.Host.HostedServices.ApiDbMigrationHostedService>();

        // Register secure configuration from appsettings
        context.Services.Configure<DeliveryApp.Application.Configuration.SecureAppSettings>(
            context.Services.GetConfiguration().GetSection("SecureAppSettings"));
        
        // Register secure password policy
        var secureSettings = new DeliveryApp.Application.Configuration.SecureAppSettings();
        context.Services.GetConfiguration().GetSection("SecureAppSettings").Bind(secureSettings);
        context.Services.AddSecurePasswordPolicy(secureSettings);

        // Register input validation service
        context.Services.AddScoped<DeliveryApp.Application.Services.InputValidationService>();

        // Register transaction management service
        context.Services.AddScoped<DeliveryApp.Application.Services.TransactionManagementService>();

        // Register secure services
        context.Services.AddScoped<DeliveryApp.Application.Services.SecureAuthService>();
        context.Services.AddScoped<DeliveryApp.Application.Services.SecureOrderService>();

        // Register global exception middleware
        context.Services.AddScoped<DeliveryApp.HttpApi.Host.Middleware.GlobalExceptionMiddleware>();
        
        // Configure antiforgery to ignore API endpoints
        context.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        {
            options.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
        });
        
        // Configure antiforgery options to be more permissive for API endpoints
        context.Services.Configure<Microsoft.AspNetCore.Antiforgery.AntiforgeryOptions>(options =>
        {
            options.SuppressXFrameOptionsHeader = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.HeaderName = "X-CSRF-TOKEN";
            options.FormFieldName = "__RequestVerificationToken";
        });
        
        // Configure OpenIddict token lifetimes
        context.Services.Configure<OpenIddictServerOptions>(options =>
        {
            options.AccessTokenLifetime = TimeSpan.FromHours(24);
            options.RefreshTokenLifetime = TimeSpan.FromDays(30);
        });

    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        // Configure authentication with OpenIddict as default scheme
        context.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });

    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }


    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<DeliveryAppHttpApiHostModule>();
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
                options.FileSets.ReplaceEmbeddedByPhysical<DeliveryAppHttpApiHostModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureAutoApiControllers()
    {
        // Remove OpenIddict default TokenController to avoid route conflicts with custom TokenController
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(DeliveryAppHttpApiHostModule).Assembly);
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

                // Resolve conflicting actions by preferring the controller over the service
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

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

    private void PrintOpenIddictConfiguration(IConfiguration configuration)
    {
        Console.WriteLine("=== OPENIDDICT CONFIGURATION DEBUG ===");
        
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        Console.WriteLine($"Environment: {environment}");
        
        // Print environment variables (may be empty for local development)
        var jwtIssuerEnv = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var jwtSecretEnv = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        var openidSecretEnv = Environment.GetEnvironmentVariable("OPENID_CLIENT_SECRET");
        var dashboardUrlEnv = Environment.GetEnvironmentVariable("DASHBOARD_URL");
        
        Console.WriteLine($"Environment Variables:");
        Console.WriteLine($"  JWT_ISSUER: {(string.IsNullOrEmpty(jwtIssuerEnv) ? "(not set - using appsettings)" : jwtIssuerEnv)}");
        Console.WriteLine($"  JWT_SECRET_KEY: {(string.IsNullOrEmpty(jwtSecretEnv) ? "(not set - using appsettings)" : jwtSecretEnv.Substring(0, Math.Min(10, jwtSecretEnv.Length)) + "...")}");
        Console.WriteLine($"  OPENID_CLIENT_SECRET: {(string.IsNullOrEmpty(openidSecretEnv) ? "(not set - using appsettings)" : openidSecretEnv.Substring(0, Math.Min(10, openidSecretEnv.Length)) + "...")}");
        Console.WriteLine($"  DASHBOARD_URL: {(string.IsNullOrEmpty(dashboardUrlEnv) ? "(not set - using appsettings)" : dashboardUrlEnv)}");

        // Print actual configuration values being used
        Console.WriteLine($"Active Configuration (from appsettings.{environment}.json):");
        Console.WriteLine($"  JWT_ISSUER: {configuration["JwtSettings:Issuer"]}");
        Console.WriteLine($"  JWT_SECRET_KEY: {configuration["JwtSettings:SecretKey"]?.Substring(0, Math.Min(10, configuration["JwtSettings:SecretKey"]?.Length ?? 0))}...");
        Console.WriteLine($"  OPENID_CLIENT_SECRET: {configuration["OpenIddict:Applications:DeliveryApp_App:ClientSecret"]?.Substring(0, Math.Min(10, configuration["OpenIddict:Applications:DeliveryApp_App:ClientSecret"]?.Length ?? 0))}...");
        Console.WriteLine($"  DASHBOARD_URL: {configuration["OpenIddict:Applications:DeliveryApp_App:RootUrl"]}");
        
        // Verify configuration is working
        var hasValidConfig = !string.IsNullOrEmpty(configuration["JwtSettings:Issuer"]) && 
                           !string.IsNullOrEmpty(configuration["JwtSettings:SecretKey"]) &&
                           !string.IsNullOrEmpty(configuration["OpenIddict:Applications:DeliveryApp_App:ClientSecret"]);
        
        Console.WriteLine($"Configuration Status: {(hasValidConfig ? "✅ VALID" : "❌ INVALID")}");
        Console.WriteLine("=== END OPENIDDICT CONFIGURATION DEBUG ===");
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        var configuration = context.GetConfiguration();

        // Debug: Print OpenIddict configuration
        PrintOpenIddictConfiguration(configuration);

        app.UseAbpRequestLocalization();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/api/error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        
        // Serve static files from uploads directory
        app.UseStaticFiles();
        
        // Ensure uploads directory exists before serving static files
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }
        
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads"
        });
        
        app.UseRouting();
        
        // Add global exception handling middleware
        app.UseMiddleware<DeliveryApp.HttpApi.Host.Middleware.GlobalExceptionMiddleware>();
        
        // Add database exception handling middleware
        app.UseMiddleware<DeliveryApp.HttpApi.Host.Middleware.DatabaseExceptionMiddleware>();
        
        // Configure API-only routing - redirect root requests to Swagger
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/" || context.Request.Path == "/Index")
            {
                // Only redirect GET requests to avoid antiforgery issues with POST
                if (context.Request.Method == "GET")
                {
                    context.Response.Redirect("/swagger");
                    return;
                }
                else
                {
                    // For non-GET requests to root/Index, return 404 or redirect to API
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("API endpoint not found. Use /swagger for API documentation.");
                    return;
                }
            }
            await next();
        });
        
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
        
        // Configure antiforgery middleware with API-friendly settings
        app.UseAntiforgery();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Waseel API");
            options.DocumentTitle = "وصيل • Waseel API Docs";
            options.HeadContent += "<link rel=\"icon\" type=\"image/png\" href=\"/swagger-ui/favicon-32x32.png\" />";
        });

        app.UseConfiguredEndpoints();
    }
}
