using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Secure order service interface with transaction management
    /// Ensures data consistency during order processing
    /// </summary>
    public interface ISecureOrderService
    {
        /// <summary>
        /// Secure order creation with transaction management
        /// </summary>
        Task<TransactionResultDto> CreateOrderSecureAsync(CreateOrderDto request);

        /// <summary>
        /// Secure order status update with transaction management
        /// </summary>
        Task<TransactionResultDto> UpdateOrderStatusSecureAsync(Guid orderId, OrderStatus newStatus, string? reason = null);

        /// <summary>
        /// Secure order cancellation with compensation
        /// </summary>
        Task<TransactionResultDto> CancelOrderSecureAsync(Guid orderId, string reason, Guid userId);

        /// <summary>
        /// Secure batch order processing with transaction management
        /// </summary>
        Task<BatchOperationResultDto<TransactionResultDto>> ProcessBatchOrdersSecureAsync(
            List<Guid> orderIds, 
            OrderStatus targetStatus, 
            string? reason = null);

        /// <summary>
        /// Secure order assignment to delivery person
        /// </summary>
        Task<TransactionResultDto> AssignOrderToDeliverySecureAsync(Guid orderId, Guid deliveryPersonId);
    }
}
