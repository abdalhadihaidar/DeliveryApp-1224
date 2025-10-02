using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryApp.Web.Controllers
{
    [RemoteService]
    [Route("api/app/admin/restaurant-owners")]
    [Authorize(Roles = "admin")]
    public class RestaurantOwnerManagementController : AbpController
    {
        private readonly IRestaurantOwnerAppService _restaurantOwnerAppService;

        public RestaurantOwnerManagementController(IRestaurantOwnerAppService restaurantOwnerAppService)
        {
            _restaurantOwnerAppService = restaurantOwnerAppService;
        }

        /// <summary>
        /// Get all restaurant owners with pagination
        /// </summary>
        [HttpGet]
        public async Task<List<RestaurantOwnerDto>> GetAllOwners(
            [FromQuery] int skipCount = 0,
            [FromQuery] int maxResultCount = 100)
        {
            return await _restaurantOwnerAppService.GetAllRestaurantOwnersAsync(skipCount, maxResultCount);
        }

        /// <summary>
        /// Get detailed information about a specific owner
        /// </summary>
        [HttpGet("{ownerId}")]
        public async Task<RestaurantOwnerDto> GetOwnerDetails(Guid ownerId)
        {
            return await _restaurantOwnerAppService.GetOwnerDetailsAsync(ownerId);
        }

        /// <summary>
        /// Get owner statistics
        /// </summary>
        [HttpGet("{ownerId}/statistics")]
        public async Task<RestaurantOwnerStatisticsDto> GetOwnerStatistics(
            Guid ownerId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return await _restaurantOwnerAppService.GetOwnerStatisticsAsync(ownerId, startDate, endDate);
        }

        /// <summary>
        /// Search and filter restaurant owners
        /// </summary>
        [HttpPost("search")]
        public async Task<List<RestaurantOwnerDto>> SearchOwners([FromBody] OwnerSearchDto searchDto)
        {
            return await _restaurantOwnerAppService.SearchOwnersAsync(
                searchDto.SearchTerm,
                searchDto.Status,
                searchDto.FromDate,
                searchDto.ToDate,
                searchDto.SkipCount,
                searchDto.MaxResultCount
            );
        }

        /// <summary>
        /// Bulk approve owners
        /// </summary>
        [HttpPost("bulk-approve")]
        public async Task<bool> BulkApproveOwners([FromBody] BulkOwnerOperationDto operationDto)
        {
            return await _restaurantOwnerAppService.BulkApproveOwnersAsync(operationDto.OwnerIds);
        }

        /// <summary>
        /// Bulk reject owners
        /// </summary>
        [HttpPost("bulk-reject")]
        public async Task<bool> BulkRejectOwners([FromBody] BulkOwnerOperationDto operationDto)
        {
            return await _restaurantOwnerAppService.BulkRejectOwnersAsync(operationDto.OwnerIds, operationDto.Reason);
        }

        /// <summary>
        /// Bulk activate owners
        /// </summary>
        [HttpPost("bulk-activate")]
        public async Task<bool> BulkActivateOwners([FromBody] BulkOwnerOperationDto operationDto)
        {
            return await _restaurantOwnerAppService.BulkActivateOwnersAsync(operationDto.OwnerIds);
        }

        /// <summary>
        /// Bulk deactivate owners
        /// </summary>
        [HttpPost("bulk-deactivate")]
        public async Task<bool> BulkDeactivateOwners([FromBody] BulkOwnerOperationDto operationDto)
        {
            return await _restaurantOwnerAppService.BulkDeactivateOwnersAsync(operationDto.OwnerIds);
        }

        /// <summary>
        /// Get owner performance metrics
        /// </summary>
        [HttpGet("{ownerId}/performance")]
        public async Task<RestaurantOwnerPerformanceDto> GetOwnerPerformance(
            Guid ownerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            return await _restaurantOwnerAppService.GetOwnerPerformanceAsync(ownerId, fromDate, toDate);
        }

        /// <summary>
        /// Get top performing owners
        /// </summary>
        [HttpGet("top-performing")]
        public async Task<List<RestaurantOwnerDto>> GetTopPerformingOwners(
            [FromQuery] int count = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            return await _restaurantOwnerAppService.GetTopPerformingOwnersAsync(count, fromDate, toDate);
        }

        /// <summary>
        /// Get owner dashboard data
        /// </summary>
        [HttpGet("{ownerId}/dashboard")]
        public async Task<RestaurantOwnerDashboardDto> GetOwnerDashboard(Guid ownerId)
        {
            return await _restaurantOwnerAppService.GetOwnerDashboardAsync(ownerId);
        }

        /// <summary>
        /// Export owners data
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportOwners([FromBody] OwnerSearchDto searchDto)
        {
            var owners = await _restaurantOwnerAppService.SearchOwnersAsync(
                searchDto.SearchTerm,
                searchDto.Status,
                searchDto.FromDate,
                searchDto.ToDate,
                0,
                10000 // Large number to get all results
            );

            var exportData = new OwnerExportDto
            {
                Owners = owners,
                ExportDate = DateTime.Now,
                ExportFormat = "CSV",
                Filters = new Dictionary<string, object>
                {
                    ["SearchTerm"] = searchDto.SearchTerm ?? "",
                    ["Status"] = searchDto.Status ?? "",
                    ["FromDate"] = searchDto.FromDate?.ToString("yyyy-MM-dd") ?? "",
                    ["ToDate"] = searchDto.ToDate?.ToString("yyyy-MM-dd") ?? ""
                }
            };

            return Ok(exportData);
        }

        /// <summary>
        /// Get owner analytics
        /// </summary>
        [HttpGet("{ownerId}/analytics")]
        public async Task<OwnerAnalyticsDto> GetOwnerAnalytics(
            Guid ownerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            // This would require additional implementation in the service
            // For now, return a basic analytics object
            return new OwnerAnalyticsDto
            {
                OwnerId = ownerId,
                OwnerName = "Owner Name", // Would be fetched from database
                FromDate = fromDate,
                ToDate = toDate,
                DailyRevenue = new List<DailyRevenueDto>(),
                MonthlyRevenue = new List<MonthlyRevenueDto>(),
                DailyOrders = new List<DailyOrderDto>(),
                HourlyOrders = new List<HourlyOrderDto>(),
                CustomerSegments = new List<CustomerSegmentDto>(),
                PerformanceTrends = new List<PerformanceTrendDto>()
            };
        }
    }
}
