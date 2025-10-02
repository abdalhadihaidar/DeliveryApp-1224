using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    /// <summary>
    /// Admin controller for delivery performance analytics and reporting
    /// </summary>
    [ApiController]
    [Route("api/app/admin/deliveries")]
    [Authorize(Roles = "admin,manager")]
    public class DeliveryPerformanceAdminController : AbpController
    {
        private readonly IDeliveryPerformanceAdminService _deliveryPerformanceService;

        public DeliveryPerformanceAdminController(IDeliveryPerformanceAdminService deliveryPerformanceService)
        {
            _deliveryPerformanceService = deliveryPerformanceService;
        }

        /// <summary>
        /// Get delivery performance data for a date range
        /// </summary>
        /// <param name="from">Start date (ISO string)</param>
        /// <param name="to">End date (ISO string)</param>
        /// <returns>Array of delivery person performance data</returns>
        [HttpGet("performance")]
        public async Task<ActionResult<DeliveryPersonPerformanceDto[]>> GetPerformance(string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return BadRequest("From and To dates are required");
            }

            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format. Use ISO 8601 format.");
            }

            var performance = await _deliveryPerformanceService.GetDeliveryPerformanceAsync(fromDate, toDate);
            return Ok(performance);
        }

        /// <summary>
        /// Get detailed performance metrics for a specific delivery person
        /// </summary>
        /// <param name="deliveryPersonId">Delivery person ID</param>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <returns>Detailed performance metrics</returns>
        [HttpGet("performance/{deliveryPersonId}")]
        public async Task<ActionResult<DetailedDeliveryPerformanceDto>> GetDeliveryPersonPerformance(
            Guid deliveryPersonId, 
            [FromQuery] string from, 
            [FromQuery] string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return BadRequest("From and To dates are required");
            }

            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format. Use ISO 8601 format.");
            }

            var performance = await _deliveryPerformanceService.GetDetailedPerformanceAsync(deliveryPersonId, fromDate, toDate);
            return Ok(performance);
        }

        /// <summary>
        /// Get delivery performance summary statistics
        /// </summary>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <returns>Performance summary statistics</returns>
        [HttpGet("performance/summary")]
        public async Task<ActionResult<DeliveryPerformanceSummaryDto>> GetPerformanceSummary(
            [FromQuery] string from, 
            [FromQuery] string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return BadRequest("From and To dates are required");
            }

            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format. Use ISO 8601 format.");
            }

            var summary = await _deliveryPerformanceService.GetPerformanceSummaryAsync(fromDate, toDate);
            return Ok(summary);
        }
    }
}
