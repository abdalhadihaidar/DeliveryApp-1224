using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using DeliveryApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;
using Volo.Abp.Domain.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Secure order service with transaction management
    /// Ensures data consistency during order processing
    /// </summary>
    public class SecureOrderService : DomainService, ISecureOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly ILogger<SecureOrderService> _logger;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;
        private readonly TransactionManagementService _transactionService;
        private readonly ICurrentUser _currentUser;

        public SecureOrderService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository,
            ILogger<SecureOrderService> logger,
            IStringLocalizer<DeliveryAppResource> localizer,
            TransactionManagementService transactionService,
            ICurrentUser currentUser)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
            _logger = logger;
            _localizer = localizer;
            _transactionService = transactionService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Secure order creation with transaction management
        /// </summary>
        public async Task<TransactionResultDto> CreateOrderSecureAsync(CreateOrderDto request)
        {
            return await _transactionService.ExecuteWithValidationAsync(
                // Validation: Check if user, restaurant, and items are valid
                async () =>
                {
                    var user = await _userRepository.GetAsync((Guid)_currentUser.Id);
                    var restaurant = await _restaurantRepository.GetAsync(request.RestaurantId);
                    
                    return user != null && user.IsActive && 
                           restaurant != null && restaurant.IsActive && 
                           request.Items != null && request.Items.Any();
                },
                // Main operation: Create order
                async () =>
                {
                    var order = new Order(GuidGenerator.Create())
                    {
                        UserId = (Guid)_currentUser.Id,
                        RestaurantId = request.RestaurantId,
                        Status = OrderStatus.Pending,
                        PaymentStatus = PaymentStatus.Pending,
                        TotalAmount = request.TotalAmount,
                        DeliveryFee = request.DeliveryFee,
                        EstimatedDeliveryTime = 30, // Default estimated delivery time
                        OrderDate = DateTime.UtcNow,
                        DeliveryAddressId = request.DeliveryAddressId,
                        Items = request.Items?.Select(item => new OrderItem
                        {
                            MenuItemId = item.MenuItemId,
                            Name = item.Name,
                            Quantity = (int)item.Quantity,
                            Price = item.Price,
                            SpecialInstructions = item.SpecialInstructions
                        }).ToList() ?? new List<OrderItem>()
                    };

                    await _orderRepository.InsertAsync(order);

                    _logger.LogInformation("Order created successfully: {OrderId} for user: {UserId}", 
                        order.Id, _currentUser.Id);

                    return new TransactionResultDto
                    {
                        Success = true,
                        Message = "Order created successfully",
                        Data = new { OrderId = order.Id },
                        CorrelationId = Guid.NewGuid().ToString()
                    };
                },
                $"CreateOrder_{_currentUser.Id}"
            );
        }

        /// <summary>
        /// Secure order status update with transaction management
        /// </summary>
        public async Task<TransactionResultDto> UpdateOrderStatusSecureAsync(Guid orderId, OrderStatus newStatus, string? reason = null)
        {
            return await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orderRepository.GetAsync(orderId);
                
                var oldStatus = order.Status;
                order.Status = newStatus;
                order.LastModificationTime = DateTime.UtcNow;

                // Add status change reason if provided
                if (!string.IsNullOrEmpty(reason))
                {
                    // You might want to store this in a separate OrderStatusHistory table
                    _logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus}. Reason: {Reason}", 
                        orderId, oldStatus, newStatus, reason);
                }

                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus}", 
                    orderId, oldStatus, newStatus);

                return new TransactionResultDto
                {
                    Success = true,
                    Message = $"Order status updated to {newStatus}",
                    Data = new { OrderId = orderId, OldStatus = oldStatus, NewStatus = newStatus },
                    CorrelationId = Guid.NewGuid().ToString()
                };
            }, $"UpdateOrderStatus_{orderId}");
        }

        /// <summary>
        /// Secure order cancellation with compensation
        /// </summary>
        public async Task<TransactionResultDto> CancelOrderSecureAsync(Guid orderId, string reason, Guid userId)
        {
            return await _transactionService.ExecuteWithCompensationAsync(
                // Main operation: Cancel order
                async () =>
                {
                    var order = await _orderRepository.GetAsync(orderId);
                    
                    if (order.UserId != userId)
                    {
                        throw new UnauthorizedAccessException("User not authorized to cancel this order");
                    }

                    if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                    {
                        throw new InvalidOperationException($"Cannot cancel order in {order.Status} status");
                    }

                    order.Status = OrderStatus.Cancelled;
                    order.LastModificationTime = DateTime.UtcNow;
                    
                    await _orderRepository.UpdateAsync(order);

                    _logger.LogInformation("Order {OrderId} cancelled by user {UserId}. Reason: {Reason}", 
                        orderId, userId, reason);

                    return new TransactionResultDto
                    {
                        Success = true,
                        Message = "Order cancelled successfully",
                        Data = new { OrderId = orderId, Reason = reason },
                        CorrelationId = Guid.NewGuid().ToString()
                    };
                },
                // Compensation: Restore order to previous status
                async () =>
                {
                    var order = await _orderRepository.GetAsync(orderId);
                    order.Status = OrderStatus.Pending; // Restore to pending
                    await _orderRepository.UpdateAsync(order);
                    
                    _logger.LogInformation("Order {OrderId} restored to pending status due to cancellation failure", orderId);
                },
                $"CancelOrder_{orderId}"
            );
        }

        /// <summary>
        /// Secure batch order processing with transaction management
        /// </summary>
        public async Task<BatchOperationResultDto<TransactionResultDto>> ProcessBatchOrdersSecureAsync(
            List<Guid> orderIds, 
            OrderStatus targetStatus, 
            string? reason = null)
        {
            var results = new List<TransactionResultDto>();
            var errors = new List<string>();

            foreach (var orderId in orderIds)
            {
                try
                {
                    var result = await UpdateOrderStatusSecureAsync(orderId, targetStatus, reason);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    errors.Add($"Order {orderId}: {ex.Message}");
                    _logger.LogError(ex, "Failed to process order {OrderId} in batch", orderId);
                }
            }

            return new BatchOperationResultDto<TransactionResultDto>
            {
                Success = errors.Count == 0,
                TotalItems = orderIds.Count,
                SuccessfulItems = results.Count,
                FailedItems = errors.Count,
                SuccessfulResults = results,
                Errors = errors,
                CorrelationId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// Secure order assignment to delivery person
        /// </summary>
        public async Task<TransactionResultDto> AssignOrderToDeliverySecureAsync(Guid orderId, Guid deliveryPersonId)
        {
            return await _transactionService.ExecuteWithValidationAsync(
                // Validation: Check if order and delivery person are valid
                async () =>
                {
                    var order = await _orderRepository.GetAsync(orderId);
                    var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                    
                    return order != null && 
                           order.Status == OrderStatus.ReadyForDelivery &&
                           deliveryPerson != null && 
                           deliveryPerson.IsActive;
                },
                // Main operation: Assign order
                async () =>
                {
                    var order = await _orderRepository.GetAsync(orderId);
                    order.DeliveryPersonId = deliveryPersonId;
                    order.Status = OrderStatus.OutForDelivery;
                    order.LastModificationTime = DateTime.UtcNow;
                    
                    await _orderRepository.UpdateAsync(order);

                    _logger.LogInformation("Order {OrderId} assigned to delivery person {DeliveryPersonId}", 
                        orderId, deliveryPersonId);

                    return new TransactionResultDto
                    {
                        Success = true,
                        Message = "Order assigned to delivery person successfully",
                        Data = new { OrderId = orderId, DeliveryPersonId = deliveryPersonId },
                        CorrelationId = Guid.NewGuid().ToString()
                    };
                },
                $"AssignOrder_{orderId}_to_{deliveryPersonId}"
            );
        }
    }
}
