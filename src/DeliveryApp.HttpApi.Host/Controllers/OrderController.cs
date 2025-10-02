using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Volo.Abp.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : AbpController
    {
        private readonly IOrderAppService _orderAppService;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;

        public OrderController(IOrderAppService orderAppService, IStringLocalizer<DeliveryAppResource> localizer)
        {
            _orderAppService = orderAppService;
            _localizer = localizer;
        }

        [HttpGet("user/{userId}")]
        public async Task<PagedResultDto<OrderDto>> GetUserOrders(Guid userId, [FromQuery] GetUserOrdersDto input)
        {
            input.UserId = userId;
            return await _orderAppService.GetUserOrdersAsync(input);
        }

        [HttpGet("my-orders")]
        public async Task<PagedResultDto<OrderDto>> GetMyOrders([FromQuery] string? status = null, [FromQuery] int skipCount = 0, [FromQuery] int maxResultCount = 50)
        {
            OrderStatus? orderStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                orderStatus = parsedStatus;
            }
            
            var input = new GetUserOrdersDto
            {
                UserId = Guid.Empty, // This will be replaced with current user's ID in the service
                Status = orderStatus,
                SkipCount = skipCount,
                MaxResultCount = maxResultCount
            };
            return await _orderAppService.GetUserOrdersAsync(input);
        }

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<PagedResultDto<OrderDto>> GetRestaurantOrders(Guid restaurantId, [FromQuery] GetRestaurantOrdersDto input)
        {
            input.RestaurantId = restaurantId;
            return await _orderAppService.GetRestaurantOrdersAsync(input);
        }

        [HttpGet("{id}")]
        public async Task<OrderDto> Get(Guid id)
        {
            return await _orderAppService.GetAsync(id);
        }

        [HttpPost]
        public async Task<OrderDto> Create([FromBody] CreateOrderDto input)
        {
            return await _orderAppService.CreateAsync(input);
        }

        [HttpPut("{id}/status")]
        public async Task<OrderDto> UpdateStatus(Guid id, [FromBody] string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                throw new ArgumentException(_localizer["General:InvalidOrderStatus", status]);
            }
            return await _orderAppService.UpdateStatusAsync(id, orderStatus);
        }
    }
}
