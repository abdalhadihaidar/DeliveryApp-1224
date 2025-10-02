using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Web.Controllers.Admin
{
    [Route("api/app/admin/deliveries/performance")]
    [Authorize(Roles = "admin")]
    public class DeliveryPerformanceAdminController : AbpController
    {
        private readonly IDeliveryPerformanceAdminService _performanceService;

        public DeliveryPerformanceAdminController(IDeliveryPerformanceAdminService performanceService)
        {
            _performanceService = performanceService;
        }

        [HttpGet]
        public Task<DeliveryPersonPerformanceDto[]> GetPerformanceAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return _performanceService.GetDeliveryPerformanceAsync(from, to);
        }

        [HttpGet("{deliveryPersonId}")]
        public Task<DetailedDeliveryPerformanceDto> GetDetailedPerformanceAsync([FromRoute] Guid deliveryPersonId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return _performanceService.GetDetailedPerformanceAsync(deliveryPersonId, from, to);
        }

        [HttpGet("summary")]
        public Task<DeliveryPerformanceSummaryDto> GetPerformanceSummaryAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return _performanceService.GetPerformanceSummaryAsync(from, to);
        }
    }
}
