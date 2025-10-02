using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using System.Collections.Generic;
using System;

namespace DeliveryApp.Web.Controllers
{
    [Route("api/dashboard")]
    [Authorize(Roles = "admin,manager")]
    public class DashboardController : DeliveryApp.Controllers.DeliveryAppController
    {
        private readonly IDashboardAppService _dashboardAppService;

        public DashboardController(IDashboardAppService dashboardAppService)
        {
            _dashboardAppService = dashboardAppService;
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "Dashboard API is working", 
                timestamp = DateTime.UtcNow,
                authenticated = User.Identity?.IsAuthenticated ?? false,
                user = User.Identity?.Name
            });
        }

        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview()
        {
            var overview = await _dashboardAppService.GetDashboardOverviewAsync();
            return Ok(overview);
        }

        [HttpGet("reviews")]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetReviews(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = null, [FromQuery] string sortOrder = null,
            [FromQuery] string storeId = null, [FromQuery] string customerId = null, [FromQuery] int? minRating = null, [FromQuery] int? maxRating = null)
        {
            var reviews = await _dashboardAppService.GetReviewsAsync(page, pageSize, sortBy, sortOrder, storeId, customerId, minRating, maxRating);
            return Ok(reviews);
        }

        [HttpGet("current-deliveries")]
        public async Task<ActionResult<PagedResultDto<CurrentDeliveryDto>>> GetCurrentDeliveries(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = null, [FromQuery] string sortOrder = null)
        {
            var deliveries = await _dashboardAppService.GetCurrentDeliveriesAsync(page, pageSize, sortBy, sortOrder);
            return Ok(deliveries);
        }

        [HttpGet("customers")]
        public async Task<ActionResult<PagedResultDto<CustomerDto>>> GetCustomers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = null, [FromQuery] string sortOrder = null,
            [FromQuery] string city = null, [FromQuery] string interactionStatus = null)
        {
            var customers = await _dashboardAppService.GetCustomersAsync(page, pageSize, sortBy, sortOrder, city, interactionStatus);
            return Ok(customers);
        }

        [HttpGet("cancelled-orders")]
        public async Task<ActionResult<PagedResultDto<CancelledOrderDto>>> GetCancelledOrders(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = null, [FromQuery] string sortOrder = null)
        {
            var cancelledOrders = await _dashboardAppService.GetCancelledOrdersAsync(page, pageSize, sortBy, sortOrder);
            return Ok(cancelledOrders);
        }

        [HttpGet("completed-orders")]
        public async Task<ActionResult<PagedResultDto<CompletedOrderDto>>> GetCompletedOrders(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = null, [FromQuery] string sortOrder = null)
        {
            var completedOrders = await _dashboardAppService.GetCompletedOrdersAsync(page, pageSize, sortBy, sortOrder);
            return Ok(completedOrders);
        }

        [HttpGet("time-difference-analysis")]
        public async Task<ActionResult<PagedResultDto<TimeDifferenceDto>>> GetTimeDifferenceAnalysis(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = null, [FromQuery] string sortOrder = null)
        {
            var timeDifferences = await _dashboardAppService.GetTimeDifferenceAnalysisAsync(page, pageSize, sortBy, sortOrder);
            return Ok(timeDifferences);
        }

        [HttpGet("stores")]
        public async Task<ActionResult<PagedResultDto<StoreDto>>> GetStores(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = null, [FromQuery] string sortOrder = null)
        {
            var stores = await _dashboardAppService.GetStoresAsync(page, pageSize, sortBy, sortOrder);
            return Ok(stores);
        }

        [HttpGet("previous-period-data")]
        public async Task<ActionResult<PreviousPeriodDataDto>> GetPreviousPeriodData()
        {
            var data = await _dashboardAppService.GetPreviousPeriodDataAsync();
            return Ok(data);
        }

        [HttpGet("recent-activities")]
        public async Task<ActionResult<List<DashboardRecentActivityDto>>> GetRecentActivities()
        {
            var activities = await _dashboardAppService.GetRecentActivitiesAsync();
            return Ok(activities);
        }
    }
}
