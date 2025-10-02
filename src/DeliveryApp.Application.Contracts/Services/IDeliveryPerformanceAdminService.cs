using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Service for delivery performance analytics and reporting for admin users
    /// </summary>
    public interface IDeliveryPerformanceAdminService : IApplicationService
    {
        /// <summary>
        /// Get delivery performance data for all delivery persons in a date range
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Array of delivery person performance data</returns>
        Task<DeliveryPersonPerformanceDto[]> GetDeliveryPerformanceAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get detailed performance metrics for a specific delivery person
        /// </summary>
        /// <param name="deliveryPersonId">Delivery person ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Detailed performance metrics</returns>
        Task<DetailedDeliveryPerformanceDto> GetDetailedPerformanceAsync(Guid deliveryPersonId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get delivery performance summary statistics
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Performance summary statistics</returns>
        Task<DeliveryPerformanceSummaryDto> GetPerformanceSummaryAsync(DateTime fromDate, DateTime toDate);
    }
}
