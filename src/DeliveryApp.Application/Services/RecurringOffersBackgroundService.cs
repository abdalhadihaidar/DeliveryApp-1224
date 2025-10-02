using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeliveryApp.Application.Services
{
    public class RecurringOffersBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringOffersBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Check every hour to reduce memory pressure

        public RecurringOffersBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RecurringOffersBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Offers Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRecurringOffersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing recurring offers");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Recurring Offers Background Service stopped");
        }

        private async Task ProcessRecurringOffersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var specialOfferService = scope.ServiceProvider.GetRequiredService<ISpecialOfferAppService>();
                
                await specialOfferService.ProcessRecurringOffersAsync();
                
                _logger.LogDebug("Recurring offers processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring offers");
            }
            finally
            {
                // Ensure scope is properly disposed
                scope?.Dispose();
            }
        }
    }
}
