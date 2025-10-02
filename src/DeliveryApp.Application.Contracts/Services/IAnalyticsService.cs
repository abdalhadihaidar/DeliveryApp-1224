using System.Threading.Tasks;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Minimal analytics service abstraction required by UserEngagementService.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Updates (increments) a real-time metric identified by a key.
        /// </summary>
        Task UpdateRealTimeMetricAsync(string key, string metric);
    }
} 
