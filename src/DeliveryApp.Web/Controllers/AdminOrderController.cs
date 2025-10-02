using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Web.Controllers
{
    [RemoteService]
    [Route("api/admin/orders")]
    [Authorize(Roles = "admin")]
    public class AdminOrderController : AbpController
    {
        private readonly IOrderAppService _orderAppService;

        public AdminOrderController(IOrderAppService orderAppService)
        {
            _orderAppService = orderAppService;
        }

        /// <summary>
        /// Get all orders with pagination and filtering for admin dashboard
        /// </summary>
        [HttpGet]
        public async Task<Application.Contracts.Dtos.PagedResultDto<OrderDto>> GetOrders(
            [FromQuery] int skipCount = 0,
            [FromQuery] int maxResultCount = 10,
            [FromQuery] string sorting = "CreationTime desc",
            [FromQuery] string status = null,
            [FromQuery] string paymentStatus = null,
            [FromQuery] string restaurantName = null,
            [FromQuery] string customerName = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] string searchTerm = null)
        {
            var input = new GetOrderListDto
            {
                SkipCount = skipCount,
                MaxResultCount = maxResultCount,
                Sorting = sorting,
                Status = status,
                PaymentStatus = paymentStatus,
                RestaurantName = restaurantName,
                CustomerName = customerName,
                DateFrom = dateFrom,
                DateTo = dateTo,
                SearchTerm = searchTerm
            };

            return await _orderAppService.GetListAsync(input);
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<OrderDto> GetOrderDetails(Guid id)
        {
            return await _orderAppService.GetAsync(id);
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<OrderDto> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto input)
        {
            return await _orderAppService.UpdateStatusAsync(id, input.Status);
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<OrderDto> CancelOrder(Guid id, [FromBody] CancelOrderDto input)
        {
            return await _orderAppService.CancelOrderAsync(id, input.CancellationReason);
        }

        /// <summary>
        /// Assign delivery person to an order
        /// </summary>
        [HttpPost("{id}/assign-delivery")]
        public async Task<OrderDto> AssignDeliveryPerson(Guid id, [FromBody] AssignDeliveryDto input)
        {
            return await _orderAppService.AssignDeliveryPersonAsync(id, input.DeliveryPersonId);
        }

        /// <summary>
        /// Get order statistics for admin dashboard
        /// </summary>
        [HttpGet("statistics")]
        public async Task<OrderStatisticsDto> GetOrderStatistics(
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            return await _orderAppService.GetOrderStatisticsAsync(dateFrom, dateTo);
        }
    }

    // DTOs for admin order management (GetOrderListDto is defined in Application.Contracts.Dtos)

    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelOrderDto
    {
        public string CancellationReason { get; set; }
        public decimal? RefundAmount { get; set; }
    }

    public class AssignDeliveryDto
    {
        public Guid DeliveryPersonId { get; set; }
    }

    // OrderStatisticsDto is defined in Application.Contracts.Dtos
}
