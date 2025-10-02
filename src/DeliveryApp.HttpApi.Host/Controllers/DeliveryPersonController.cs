using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [RemoteService]
    [Route("api/delivery")]
    [Authorize(Roles = "delivery")]
    public class DeliveryPersonController : AbpController
    {
        private readonly IDeliveryPersonAppService _deliveryPersonAppService;

        public DeliveryPersonController(IDeliveryPersonAppService deliveryPersonAppService)
        {
            _deliveryPersonAppService = deliveryPersonAppService;
        }

        [HttpGet("profile")]
        public async Task<DeliveryPersonDto> GetProfile()
        {
            return await _deliveryPersonAppService.GetProfileAsync();
        }

        [HttpPut("profile")]
        public async Task<DeliveryPersonDto> UpdateProfile([FromBody] UpdateUserDto input)
        {
            return await _deliveryPersonAppService.UpdateProfileAsync(input);
        }

        [HttpPut("availability")]
        public async Task<bool> SetAvailability([FromBody] bool isAvailable)
        {
            return await _deliveryPersonAppService.SetAvailabilityAsync(isAvailable);
        }

        [HttpPut("location")]
        public async Task<bool> UpdateLocation([FromBody] LocationUpdateDto input)
        {
            return await _deliveryPersonAppService.UpdateLocationAsync(input.Latitude, input.Longitude);
        }

        [HttpGet("orders/available")]
        public async Task<List<OrderDto>> GetAvailableOrders()
        {
            return await _deliveryPersonAppService.GetAvailableOrdersAsync();
        }

        [HttpGet("orders/assigned")]
        public async Task<List<OrderDto>> GetAssignedOrders()
        {
            return await _deliveryPersonAppService.GetAssignedOrdersAsync();
        }

        [HttpPost("orders/{orderId}/accept")]
        public async Task<OrderDto> AcceptOrder(Guid orderId)
        {
            return await _deliveryPersonAppService.AcceptOrderAsync(orderId);
        }

        [HttpPut("orders/{orderId}/status")]
        public async Task<OrderDto> UpdateOrderStatus(Guid orderId, [FromBody] OrderStatus status)
        {
            return await _deliveryPersonAppService.UpdateOrderStatusAsync(orderId, status);
        }

        [HttpPost("orders/{orderId}/complete")]
        public async Task<OrderDto> CompleteOrder(Guid orderId)
        {
            return await _deliveryPersonAppService.CompleteOrderAsync(orderId);
        }

        [HttpGet("statistics")]
        public async Task<DeliveryStatisticsDto> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _deliveryPersonAppService.GetDeliveryStatisticsAsync(startDate, endDate);
        }
    }

    public class LocationUpdateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
