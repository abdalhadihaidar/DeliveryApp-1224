using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Services
{
    [Authorize(Roles = "delivery")]
    public class DeliveryPersonAppService : ApplicationService, IDeliveryPersonAppService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IdentityUserManager _userManager;
        private readonly ICurrentUser _currentUser;
        private readonly IOrderStatusNotificationService _orderStatusNotifier;
        private readonly IDeliveryAssignmentService _deliveryAssignmentService;
        private readonly ICODService _codService;

        public DeliveryPersonAppService(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IdentityUserManager userManager,
            ICurrentUser currentUser,
            IOrderStatusNotificationService orderStatusNotifier,
            IDeliveryAssignmentService deliveryAssignmentService,
            ICODService codService)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _currentUser = currentUser;
            _orderStatusNotifier = orderStatusNotifier;
            _deliveryAssignmentService = deliveryAssignmentService;
            _codService = codService;
        }

        private Guid GetCurrentUserId()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            
            var userId = _currentUser.GetId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User ID not found in authentication token");
            }
            
            return userId;
        }

        public async Task<DeliveryPersonDto> GetProfileAsync()
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            
            var deliveryPersonDto = ObjectMapper.Map<AppUser, DeliveryPersonDto>(user);
            
            // Get current deliveries
            var currentDeliveries = await _orderRepository.GetListAsync(
                o => o.Status == OrderStatus.Delivering && o.DeliveryPersonId == userId);
            
            deliveryPersonDto.CurrentDeliveries = ObjectMapper.Map<List<Order>, List<OrderSummaryDto>>(currentDeliveries);
            
            return deliveryPersonDto;
        }

        public async Task<DeliveryPersonDto> UpdateProfileAsync(UpdateUserDto input)
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            
            user.Name = input.Name;
            user.SetPhoneNumber(input.PhoneNumber, false);
            user.ProfileImageUrl = input.ProfileImageUrl;
            
            await _userRepository.UpdateAsync(user);
            
            return await GetProfileAsync();
        }

        public async Task<bool> SetAvailabilityAsync(bool isAvailable)
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetAsync(userId);
            
            // Get current location or use default
            var latitude = user?.CurrentLocation?.Latitude ?? 0.0;
            var longitude = user?.CurrentLocation?.Longitude ?? 0.0;
            
            // Update availability and location
            return await _deliveryAssignmentService.UpdateDeliveryPersonLocationAsync(userId, latitude, longitude, isAvailable);
        }

        public async Task<bool> UpdateLocationAsync(double latitude, double longitude)
        {
            var userId = GetCurrentUserId();
            
            // Get current availability status
            var user = await _userRepository.GetAsync(userId);
            var isAvailable = user?.DeliveryStatus?.IsAvailable ?? true;
            
            // Update location using the delivery assignment service
            return await _deliveryAssignmentService.UpdateDeliveryPersonLocationAsync(userId, latitude, longitude, isAvailable);
        }

        public async Task<List<OrderDto>> GetAvailableOrdersAsync()
        {
            // Get orders that are ready for delivery (status = "جاهز للتوصيل")
            var availableOrders = await _orderRepository.GetListAsync(
                o => o.Status == OrderStatus.ReadyForDelivery && o.DeliveryPersonId == null);
            
            return ObjectMapper.Map<List<Order>, List<OrderDto>>(availableOrders);
        }

        public async Task<List<OrderDto>> GetAssignedOrdersAsync()
        {
            var userId = GetCurrentUserId();
            
            // Get orders assigned to this delivery person
            var assignedOrders = await _orderRepository.GetListAsync(
                o => o.DeliveryPersonId == userId && o.Status == OrderStatus.Delivering);
            
            return ObjectMapper.Map<List<Order>, List<OrderDto>>(assignedOrders);
        }

        public async Task<OrderDto> AcceptOrderAsync(Guid orderId)
        {
            var userId = GetCurrentUserId();
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            
            // Check if the order is available for delivery
            if (order.Status != OrderStatus.ReadyForDelivery || order.DeliveryPersonId != null)
            {
                throw new InvalidOperationException("This order is not available for delivery.");
            }
            
            var previousStatus = order.Status;
            // Assign the order to this delivery person
            order.DeliveryPersonId = userId;
            order.Status = OrderStatus.Delivering; // In delivery
            
            await _orderRepository.UpdateAsync(order);
            
            // Send comprehensive notifications about the delivery assignment
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(orderId, previousStatus, OrderStatus.Delivering, userId);
            await _orderStatusNotifier.NotifyDeliveryAssignmentAsync(orderId, userId);
            
            return ObjectMapper.Map<Order, OrderDto>(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
        {
            var userId = GetCurrentUserId();
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            
            // Verify the order is assigned to this delivery person
            if (order.DeliveryPersonId != userId)
            {
                throw new UnauthorizedAccessException("You are not assigned to this order.");
            }
            
            var previousStatus = order.Status;
            // Update the status
            order.Status = status;
            
            await _orderRepository.UpdateAsync(order);
            
            // Send comprehensive notifications to all relevant parties
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(orderId, previousStatus, status, userId);
            
            return ObjectMapper.Map<Order, OrderDto>(order);
        }

        public async Task<OrderDto> CompleteOrderAsync(Guid orderId)
        {
            var userId = GetCurrentUserId();
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            
            // Verify the order is assigned to this delivery person
            if (order.DeliveryPersonId != userId)
            {
                throw new UnauthorizedAccessException("You are not assigned to this order.");
            }
            
            // Check if the order is in delivery
            if (order.Status != OrderStatus.Delivering)
            {
                throw new InvalidOperationException("Only orders in delivery can be completed.");
            }
            
            var previousStatus = order.Status;
            // Complete the order
            order.Status = OrderStatus.Delivered; // Completed
            
            await _orderRepository.UpdateAsync(order);
            
            // Send comprehensive notifications to all relevant parties
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(orderId, previousStatus, OrderStatus.Delivered, userId);
            
            return ObjectMapper.Map<Order, OrderDto>(order);
        }

        public async Task<DeliveryStatisticsDto> GetDeliveryStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = GetCurrentUserId();
            
            // Set default date range if not provided
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;
            
            // Get all orders delivered by this person
            var deliveredOrders = await _orderRepository.GetListAsync(
                o => o.DeliveryPersonId == userId && 
                     o.OrderDate >= startDate && 
                     o.OrderDate <= endDate);
            
            var completedOrders = deliveredOrders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            
            // Calculate statistics
            var statistics = new DeliveryStatisticsDto
            {
                TotalDeliveries = deliveredOrders.Count,
                CompletedDeliveries = completedOrders.Count,
                TotalEarnings = completedOrders.Sum(o => o.DeliveryFee), // In a real app, you'd have a delivery fee or commission
                AverageRating = 4.5, // Mock value - in a real app, you'd calculate from ratings
                AverageDeliveryTime = 30 // Mock value - in a real app, you'd calculate from delivery times
            };
            
            return statistics;
        }

        // COD-related methods
        public async Task<decimal> GetCashBalanceAsync()
        {
            var userId = GetCurrentUserId();
            return await _codService.GetCashBalanceAsync(userId);
        }

        public async Task<bool> UpdateCashBalanceAsync(decimal amount, string reason)
        {
            var userId = GetCurrentUserId();
            return await _codService.UpdateCashBalanceAsync(userId, amount, reason);
        }

        public async Task<List<CODTransactionDto>> GetCODTransactionsAsync(int skipCount = 0, int maxResultCount = 10)
        {
            var userId = GetCurrentUserId();
            return await _codService.GetTransactionsByDeliveryPersonAsync(userId, skipCount, maxResultCount);
        }

        public async Task<CODPaymentResultDto> ProcessCODPaymentAsync(Guid orderId)
        {
            var userId = GetCurrentUserId();
            return await _codService.ProcessCODPaymentAsync(orderId, userId);
        }

        public async Task<bool> SetCODPreferencesAsync(bool acceptsCOD, decimal maxCashLimit)
        {
            var userId = GetCurrentUserId();
            return await _codService.SetCODPreferencesAsync(userId, acceptsCOD, maxCashLimit);
        }

        public async Task<CODPreferencesDto> GetCODPreferencesAsync()
        {
            var userId = GetCurrentUserId();
            var balance = await _codService.GetCashBalanceAsync(userId);
            var user = await _userRepository.GetAsync(userId);
            
            return new CODPreferencesDto
            {
                DeliveryPersonId = userId,
                AcceptsCOD = user?.DeliveryStatus?.AcceptsCOD ?? false,
                MaxCashLimit = user?.DeliveryStatus?.MaxCashLimit ?? 1000,
                CurrentBalance = balance
            };
        }

        public async Task<bool> HasSufficientCashForOrderAsync(Guid orderId)
        {
            var userId = GetCurrentUserId();
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null) return false;
            
            return await _codService.HasSufficientCashBalanceAsync(userId, order.TotalAmount);
        }
    }
}
