using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DeliveryApp.Data;

namespace DeliveryApp.HttpApi.Host.HostedServices;

/// <summary>
/// Ensures that the database schema is up-to-date and seeded every time the HTTP API host starts.
/// </summary>
public class ApiDbMigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApiDbMigrationHostedService> _logger;

    public ApiDbMigrationHostedService(IServiceProvider serviceProvider, ILogger<ApiDbMigrationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<DeliveryAppDbMigrationService>();
        _logger.LogInformation("Applying database migrations/seed on API startup …");
        await migrationService.MigrateAsync();
        _logger.LogInformation("Database ready.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
