using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Enums;

using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IOrderAppService : IApplicationService
    {
        Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(GetUserOrdersDto input);
        Task<PagedResultDto<OrderDto>> GetRestaurantOrdersAsync(GetRestaurantOrdersDto input);
        Task<OrderDto> GetAsync(Guid id);
        Task<OrderDto> CreateAsync(CreateOrderDto input);
        Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus status);
        
        // Admin methods
        Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListDto input);
        Task<OrderDto> CancelOrderAsync(Guid id, string cancellationReason = null);
        Task<OrderDto> AssignDeliveryPersonAsync(Guid id, Guid deliveryPersonId);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    }
}
