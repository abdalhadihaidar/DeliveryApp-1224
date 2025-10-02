using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DeliveryApp.Data;

namespace DeliveryApp.Web.HostedServices
{
    /// <summary>
    /// Background service that executes DeliveryAppDbMigrationService when the web app starts.
    /// Ensures the database schema is up-to-date and all seed data is inserted on every run.
    /// </summary>
    public class WebDbMigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebDbMigrationHostedService> _logger;

        public WebDbMigrationHostedService(IServiceProvider serviceProvider, ILogger<WebDbMigrationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<DeliveryAppDbMigrationService>();

            _logger.LogInformation("Starting database migration from WebDbMigrationHostedService ...");
            await migrationService.MigrateAsync();
            _logger.LogInformation("Database migration completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
} 
