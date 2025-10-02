using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DeliveryApp.HttpApi.Host;

public class Program
{
    public async static Task<int> Main(string[] args)
    {

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting HTTP API host.");
            var builder = WebApplication.CreateBuilder(args);
            
            // Configure URLs based on environment
            var environment = builder.Environment.EnvironmentName;
            Log.Information($"Environment: {environment}");
            
            if (environment == "Development")
            {
                // For development, bind to localhost with specific ports
                builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");
            }
            else
            {
                // For production/deployment, let IIS handle the binding
                // The web.config will set ASPNETCORE_URLS environment variable
                Log.Information("Production environment detected - using IIS binding");
            }
            
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            
            // Add environment variables to configuration (with prefix for security)
            builder.Configuration.AddEnvironmentVariables();
            
            // Ensure proper configuration loading order:
            // 1. appsettings.json (base configuration)
            // 2. appsettings.{Environment}.json (environment-specific overrides)
            // 3. Environment variables (highest priority)
            Log.Information($"Loading configuration for environment: {builder.Environment.EnvironmentName}");
            await builder.AddApplicationAsync<DeliveryAppHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
