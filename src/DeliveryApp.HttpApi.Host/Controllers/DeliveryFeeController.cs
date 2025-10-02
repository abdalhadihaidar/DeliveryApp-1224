using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    /// <summary>
    /// Controller for delivery fee calculation and management
    /// </summary>
    [ApiController]
    [Route("api/delivery-fee")]
    public class DeliveryFeeController : AbpControllerBase
    {
        private readonly IDeliveryFeeCalculationService _deliveryFeeCalculationService;

        public DeliveryFeeController(IDeliveryFeeCalculationService deliveryFeeCalculationService)
        {
            _deliveryFeeCalculationService = deliveryFeeCalculationService;
        }

        /// <summary>
        /// Calculate delivery fee for an order
        /// </summary>
        /// <param name="request">Delivery fee calculation request</param>
        /// <returns>Delivery fee calculation result</returns>
        [HttpPost("calculate")]
        public async Task<DeliveryFeeCalculationResultDto> CalculateDeliveryFeeAsync([FromBody] DeliveryFeeCalculationRequestDto request)
        {
            return await _deliveryFeeCalculationService.CalculateDeliveryFeeAsync(request);
        }

        /// <summary>
        /// Get available delivery fee options for a location
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="customerAddressId">Customer address ID</param>
        /// <returns>Available delivery fee options</returns>
        [HttpGet("options")]
        public async Task<DeliveryFeeOptionsDto> GetDeliveryFeeOptionsAsync([FromQuery] Guid restaurantId, [FromQuery] Guid customerAddressId)
        {
            return await _deliveryFeeCalculationService.GetDeliveryFeeOptionsAsync(restaurantId, customerAddressId);
        }
    }
}
