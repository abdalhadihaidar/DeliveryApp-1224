using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using DeliveryApp.Domain.Repositories;
using System.Linq;
using Volo.Abp.Domain.Repositories;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Services;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;


namespace DeliveryApp.Application.Services
{
    public class OrderAppService : ApplicationService, IOrderAppService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IRepository<MenuItem, Guid> _menuItemRepository;
        private readonly INotificationService _notificationService;
        private readonly IOrderStatusNotificationService _orderStatusNotifier;
        private readonly IDeliveryFeeCalculationService _deliveryFeeCalculationService;

        public OrderAppService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository,
            IRepository<MenuItem, Guid> menuItemRepository,
            INotificationService notificationService,
            IOrderStatusNotificationService orderStatusNotifier,
            IDeliveryFeeCalculationService deliveryFeeCalculationService)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
            _menuItemRepository = menuItemRepository;
            _notificationService = notificationService;
            _orderStatusNotifier = orderStatusNotifier;
            _deliveryFeeCalculationService = deliveryFeeCalculationService;
        }

        public async Task<List<OrderDto>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetListAsync();
            return ObjectMapper.Map<List<Domain.Entities.Order>, List<OrderDto>>(orders);
        }

        public async Task<OrderDto> GetAsync(Guid id)
        {
            var order = await _orderRepository.GetAsync(id);
            return ObjectMapper.Map<Domain.Entities.Order, OrderDto>(order);
        }

        public async Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(GetUserOrdersDto input)
        {
            // Validate that the current user is authenticated
            if (!CurrentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User must be authenticated to view orders.");
            }

            // If no UserId is provided, use the current user's ID
            if (input.UserId == Guid.Empty && CurrentUser.Id.HasValue)
            {
                input.UserId = CurrentUser.Id.Value;
            }

            var orders = await _orderRepository.GetUserOrdersAsync(
                input.UserId,
                input.Status,
                input.SkipCount,
                input.MaxResultCount);

            var orderDtos = ObjectMapper.Map<List<Domain.Entities.Order>, List<OrderDto>>(orders);
            var totalCount = orderDtos.Count;
            return new PagedResultDto<OrderDto>(totalCount, orderDtos);
        }

        public async Task<PagedResultDto<OrderDto>> GetRestaurantOrdersAsync(GetRestaurantOrdersDto input)
        {
            var orders = await _orderRepository.GetRestaurantOrdersAsync(
                input.RestaurantId,
                input.Status,
                input.SkipCount,
                input.MaxResultCount);

            var orderDtos = ObjectMapper.Map<List<Domain.Entities.Order>, List<OrderDto>>(orders);
            var totalCount = orderDtos.Count;
            return new PagedResultDto<OrderDto>(totalCount, orderDtos);
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto input)
        {
            var order = new Domain.Entities.Order
            {
                RestaurantId = input.RestaurantId,
                UserId = CurrentUser.Id ?? Guid.Empty,
                DeliveryAddressId = input.DeliveryAddressId,
                PaymentMethodId = input.PaymentMethodId,
                Status = OrderStatus.Pending
            };

            // Add order items
            order.Items = new List<Domain.Entities.OrderItem>();
            foreach (var itemDto in input.Items)
            {
                var menuItem = await _menuItemRepository.GetAsync(itemDto.MenuItemId);
                var orderItem = new Domain.Entities.OrderItem
                {
                    MenuItemId = itemDto.MenuItemId,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = (int)itemDto.Quantity,
                    SelectedOptions = itemDto.SelectedOptions,
                    SpecialInstructions = itemDto.SpecialInstructions
                };
                order.Items.Add(orderItem);
            }

            // Calculate totals
            order.Subtotal = order.Items.Sum(i => i.Price * i.Quantity);
            
            // Calculate delivery fee using the new dynamic calculation service
            var deliveryFeeRequest = new DeliveryFeeCalculationRequestDto
            {
                OrderId = order.Id,
                RestaurantId = input.RestaurantId,
                CustomerAddressId = input.DeliveryAddressId,
                OrderAmount = order.Subtotal,
                IsRushDelivery = input.IsRushDelivery ?? false,
                PreferredDeliveryTime = input.PreferredDeliveryTime
            };

            var deliveryFeeResult = await _deliveryFeeCalculationService.CalculateDeliveryFeeAsync(deliveryFeeRequest);
            
            if (!deliveryFeeResult.Success)
            {
                throw new InvalidOperationException($"Delivery fee calculation failed: {deliveryFeeResult.Message}");
            }

            order.DeliveryFee = deliveryFeeResult.DeliveryFee;
            order.Tax = order.Subtotal * 0.08m; // Example 8% tax
            order.TotalAmount = order.Subtotal + order.DeliveryFee + order.Tax;

            // Save order
            await _orderRepository.InsertAsync(order);

            // Send real-time notification to restaurant
            await _notificationService.SendOrderNotificationToRestaurantAsync(
                order.RestaurantId.ToString(),
                order.Id.ToString(),
                $"New order received! Order #{order.Id} - Total: ${order.TotalAmount:F2}"
            );

            // Send order confirmation to customer
            await _notificationService.SendOrderStatusUpdateAsync(
                order.Id.ToString(),
                "Pending",
                "Your order has been placed and is waiting for restaurant confirmation."
            );

            return ObjectMapper.Map<Domain.Entities.Order, OrderDto>(order);
        }

        public async Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _orderRepository.GetAsync(id);
            var previousStatus = order.Status;
            order.Status = status;

            if (status == OrderStatus.Delivering)
            {
                order.EstimatedDeliveryTime = 30; // 30 minutes delivery time
            }

            await _orderRepository.UpdateAsync(order);

            // Send comprehensive notifications to all relevant parties
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(id, previousStatus, status);

            return ObjectMapper.Map<Domain.Entities.Order, OrderDto>(order);
        }

        private string GetStatusMessage(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Your order is waiting for restaurant confirmation.",
                OrderStatus.Preparing => "Your order is currently being prepared.",
                OrderStatus.ReadyForDelivery => "Your order is ready and waiting for a delivery person.",
                OrderStatus.WaitingCourier => "Your order is waiting for courier assignment.",
                OrderStatus.Delivering => "Your order is on its way to you.",
                OrderStatus.Delivered => "Your order has been delivered successfully.",
                OrderStatus.Cancelled => "Your order has been cancelled.",
                _ => "Order status updated."
            };
        }


        // Admin methods
        public async Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListDto input)
        {
            // Get all orders and filter in application layer
            var allOrders = await _orderRepository.GetListAsync();
            
            // Apply filters
            var filteredOrders = allOrders.AsQueryable();
            
            if (!string.IsNullOrEmpty(input.Status) && Enum.TryParse<OrderStatus>(input.Status, true, out var status))
            {
                filteredOrders = filteredOrders.Where(o => o.Status == status);
            }
            
            if (!string.IsNullOrEmpty(input.RestaurantName))
            {
                // Note: This would require joining with restaurant data in a real implementation
                // For now, we'll skip this filter or implement it differently
            }
            
            if (!string.IsNullOrEmpty(input.CustomerName))
            {
                // Note: This would require joining with user data in a real implementation
                // For now, we'll skip this filter or implement it differently
            }
            
            if (input.DateFrom.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.CreationTime >= input.DateFrom.Value);
            }
            
            if (input.DateTo.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.CreationTime <= input.DateTo.Value);
            }
            
            if (!string.IsNullOrEmpty(input.SearchTerm))
            {
                // Note: This would require searching across multiple fields in a real implementation
                // For now, we'll skip this filter
            }
            
            // Apply sorting
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                if (input.Sorting.Contains("CreationTime desc"))
                {
                    filteredOrders = filteredOrders.OrderByDescending(o => o.CreationTime);
                }
                else if (input.Sorting.Contains("CreationTime asc"))
                {
                    filteredOrders = filteredOrders.OrderBy(o => o.CreationTime);
                }
            }
            
            var totalCount = filteredOrders.Count();
            var orders = filteredOrders.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            
            var orderDtos = ObjectMapper.Map<List<Domain.Entities.Order>, List<OrderDto>>(orders);
            
            return new PagedResultDto<OrderDto>(totalCount, orderDtos);
        }

        public async Task<OrderDto> CancelOrderAsync(Guid id, string cancellationReason = null)
        {
            var order = await _orderRepository.GetAsync(id);
            
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot cancel an order that is already delivered or cancelled.");
            }

            var previousStatus = order.Status;
            order.Status = OrderStatus.Cancelled;
            // Add cancellation reason to order if there's a field for it
            // order.CancellationReason = cancellationReason;
            
            await _orderRepository.UpdateAsync(order);

            // Send notifications
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(id, previousStatus, OrderStatus.Cancelled);

            return ObjectMapper.Map<Domain.Entities.Order, OrderDto>(order);
        }

        public async Task<OrderDto> AssignDeliveryPersonAsync(Guid id, Guid deliveryPersonId)
        {
            var order = await _orderRepository.GetAsync(id);
            
            if (order.Status != OrderStatus.ReadyForDelivery)
            {
                throw new InvalidOperationException("Can only assign delivery person to orders that are ready for delivery.");
            }

            order.DeliveryPersonId = deliveryPersonId;
            order.Status = OrderStatus.OutForDelivery;
            
            await _orderRepository.UpdateAsync(order);

            // Send notifications
            await _orderStatusNotifier.NotifyOrderStatusChangeAsync(id, OrderStatus.ReadyForDelivery, OrderStatus.OutForDelivery);

            return ObjectMapper.Map<Domain.Entities.Order, OrderDto>(order);
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var orders = await _orderRepository.GetListAsync();
            
            // Filter by date range if provided
            if (dateFrom.HasValue || dateTo.HasValue)
            {
                orders = orders.Where(o => 
                    (!dateFrom.HasValue || o.CreationTime >= dateFrom.Value) &&
                    (!dateTo.HasValue || o.CreationTime <= dateTo.Value)
                ).ToList();
            }

            var statistics = new OrderStatisticsDto
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ConfirmedOrders = orders.Count(o => o.Status == OrderStatus.Confirmed),
                PreparingOrders = orders.Count(o => o.Status == OrderStatus.Preparing),
                ReadyOrders = orders.Count(o => o.Status == OrderStatus.ReadyForDelivery),
                OutForDeliveryOrders = orders.Count(o => o.Status == OrderStatus.OutForDelivery),
                DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                OverdueOrders = orders.Count(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled && 
                    o.CreationTime.AddMinutes(o.EstimatedDeliveryTime) < DateTime.Now),
                AverageDeliveryTime = orders.Where(o => o.Status == OrderStatus.Delivered).Any() ? 
                    orders.Where(o => o.Status == OrderStatus.Delivered).Average(o => o.EstimatedDeliveryTime) : 0
            };

            return statistics;
        }
    }
}

