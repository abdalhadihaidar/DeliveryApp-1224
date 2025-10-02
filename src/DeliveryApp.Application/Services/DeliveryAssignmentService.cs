using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Intelligent delivery assignment service with location-based nearest delivery logic
    /// </summary>
    public class DeliveryAssignmentService : ApplicationService, IDeliveryAssignmentService, ITransientDependency
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderStatusNotificationService _notificationService;
        private readonly IdentityUserManager _userManager;
        private readonly ICODService _codService;
        private readonly ILogger<DeliveryAssignmentService> _logger;

        public DeliveryAssignmentService(
            IOrderRepository orderRepository,
            IRestaurantRepository restaurantRepository,
            IUserRepository userRepository,
            IOrderStatusNotificationService notificationService,
            IdentityUserManager userManager,
            ICODService codService,
            ILogger<DeliveryAssignmentService> logger)
        {
            _orderRepository = orderRepository;
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _userManager = userManager;
            _codService = codService;
            _logger = logger;
        }

        public async Task<DeliveryAssignmentResultDto> AssignNearestDeliveryPersonAsync(Guid orderId, double maxRadiusKm = 10.0)
        {
            try
            {
                // Get order with details
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = "Order not found",
                        ErrorCode = "ORDER_NOT_FOUND"
                    };
                }

                // Check if order is in correct status for delivery assignment
                if (order.Status != OrderStatus.ReadyForDelivery)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = $"Order status must be 'ReadyForDelivery' for assignment. Current status: {order.Status}",
                        ErrorCode = "INVALID_ORDER_STATUS"
                    };
                }

                // Get restaurant location
                var restaurant = await _restaurantRepository.GetWithAddressAsync(order.RestaurantId);
                if (restaurant?.Address == null)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = "Restaurant location not found",
                        ErrorCode = "RESTAURANT_LOCATION_MISSING"
                    };
                }

                // Find available delivery persons near the restaurant
                var availableDeliveryPersons = await GetAvailableDeliveryPersonsAsync(
                    restaurant.Address.Latitude, 
                    restaurant.Address.Longitude, 
                    maxRadiusKm,
                    orderId);

                if (!availableDeliveryPersons.Any())
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = $"No available delivery persons found within {maxRadiusKm}km radius",
                        ErrorCode = "NO_AVAILABLE_DELIVERY_PERSONS"
                    };
                }

                // Get the nearest available delivery person (already sorted by distance)
                var nearestDeliveryPerson = availableDeliveryPersons.First();

                // Assign the order
                var assignmentResult = await ManualAssignDeliveryPersonAsync(orderId, nearestDeliveryPerson.Id);
                
                if (assignmentResult.Success)
                {
                    assignmentResult.DistanceKm = nearestDeliveryPerson.DistanceKm;
                    _logger.LogInformation($"Order {orderId} assigned to delivery person {nearestDeliveryPerson.Id} ({nearestDeliveryPerson.Name}) at {nearestDeliveryPerson.DistanceKm:F2}km distance");
                }

                return assignmentResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning nearest delivery person for order {orderId}");
                return new DeliveryAssignmentResultDto
                {
                    Success = false,
                    Message = "Internal error during assignment",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<List<AvailableDeliveryPersonDto>> GetAvailableDeliveryPersonsAsync(double latitude, double longitude, double radiusKm = 10.0, Guid? orderId = null)
        {
            try
            {
                // Get all users with delivery role
                var deliveryPersons = await _userRepository.GetUsersByRoleAsync("delivery");

                var availableDeliveryPersons = new List<AvailableDeliveryPersonDto>();

                foreach (var deliveryPerson in deliveryPersons)
                {
                    // Check if delivery person has location data
                    if (deliveryPerson.CurrentLocation == null) continue;

                    // Calculate distance using Haversine formula
                    var distance = CalculateDistanceKm(
                        latitude, longitude,
                        deliveryPerson.CurrentLocation.Latitude,
                        deliveryPerson.CurrentLocation.Longitude);

                    // Skip if outside radius
                    if (distance > radiusKm) continue;

                    // Check availability
                    var isAvailable = await IsDeliveryPersonAvailable(deliveryPerson.Id);
                    if (!isAvailable) continue;

                    // Get current active orders count
                    var activeOrdersCount = await GetActiveOrdersCount(deliveryPerson.Id);

                    // Skip if already has too many orders (max 3 concurrent)
                    if (activeOrdersCount >= 3) continue;

                    // Get delivery person stats
                    var completedDeliveries = await GetCompletedDeliveriesCount(deliveryPerson.Id);
                    var rating = await GetDeliveryPersonRating(deliveryPerson.Id);

                    // Get COD-related information
                    var acceptsCOD = deliveryPerson.DeliveryStatus?.AcceptsCOD ?? false;
                    var cashBalance = deliveryPerson.DeliveryStatus?.CashBalance ?? 0;
                    var maxCashLimit = deliveryPerson.DeliveryStatus?.MaxCashLimit ?? 1000;
                    
                    // Check COD requirements if orderId is provided
                    decimal orderAmount = 0;
                    bool hasSufficientCashForOrder = true;
                    
                    if (orderId.HasValue)
                    {
                        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId.Value);
                        if (order != null)
                        {
                            orderAmount = order.TotalAmount;
                            
                            // Check if this is a COD order
                            var isCODOrder = order.PaymentMethod?.Type == PaymentType.CashOnDelivery;
                            
                            if (isCODOrder)
                            {
                                // For COD orders, check if driver has sufficient cash balance
                                hasSufficientCashForOrder = await _codService.HasSufficientCashBalanceAsync(deliveryPerson.Id, orderAmount);
                                
                                // Skip drivers who don't accept COD or don't have sufficient cash
                                if (!acceptsCOD || !hasSufficientCashForOrder)
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    availableDeliveryPersons.Add(new AvailableDeliveryPersonDto
                    {
                        Id = deliveryPerson.Id,
                        Name = deliveryPerson.Name ?? string.Empty,
                        Email = deliveryPerson.Email ?? string.Empty,
                        PhoneNumber = deliveryPerson.PhoneNumber ?? string.Empty,
                        ProfileImageUrl = deliveryPerson.ProfileImageUrl ?? string.Empty,
                        Latitude = deliveryPerson.CurrentLocation.Latitude,
                        Longitude = deliveryPerson.CurrentLocation.Longitude,
                        DistanceKm = distance,
                        IsAvailable = true,
                        CurrentActiveOrders = activeOrdersCount,
                        LastLocationUpdate = deliveryPerson.CurrentLocation.LastModificationTime ?? DateTime.UtcNow,
                        Rating = rating,
                        CompletedDeliveries = completedDeliveries,
                        
                        // COD-related fields
                        AcceptsCOD = acceptsCOD,
                        CashBalance = cashBalance,
                        MaxCashLimit = maxCashLimit,
                        HasSufficientCashForOrder = hasSufficientCashForOrder,
                        OrderAmount = orderAmount
                    });
                }

                // Sort by distance, then by rating, then by active orders count
                return availableDeliveryPersons
                    .OrderBy(dp => dp.DistanceKm)
                    .ThenByDescending(dp => dp.Rating)
                    .ThenBy(dp => dp.CurrentActiveOrders)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available delivery persons");
                return new List<AvailableDeliveryPersonDto>();
            }
        }

        public async Task<DeliveryAssignmentResultDto> ManualAssignDeliveryPersonAsync(Guid orderId, Guid deliveryPersonId)
        {
            try
            {
                // Get order
                var order = await _orderRepository.GetAsync(orderId);
                if (order == null)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = "Order not found",
                        ErrorCode = "ORDER_NOT_FOUND"
                    };
                }

                // Check order status
                if (order.Status != OrderStatus.ReadyForDelivery)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = $"Order must be in 'ReadyForDelivery' status for assignment. Current: {order.Status}",
                        ErrorCode = "INVALID_ORDER_STATUS"
                    };
                }

                // Get delivery person
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                if (deliveryPerson == null)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = "Delivery person not found",
                        ErrorCode = "DELIVERY_PERSON_NOT_FOUND"
                    };
                }

                // Check if delivery person is available
                var isAvailable = await IsDeliveryPersonAvailable(deliveryPersonId);
                if (!isAvailable)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = "Delivery person is not available",
                        ErrorCode = "DELIVERY_PERSON_NOT_AVAILABLE"
                    };
                }

                // Check if delivery person already has too many active orders
                var activeOrdersCount = await GetActiveOrdersCount(deliveryPersonId);
                if (activeOrdersCount >= 3)
                {
                    return new DeliveryAssignmentResultDto
                    {
                        Success = false,
                        Message = "Delivery person has reached maximum concurrent orders limit",
                        ErrorCode = "MAX_ORDERS_EXCEEDED"
                    };
                }

                // Assign delivery person to order
                var previousStatus = order.Status;
                order.DeliveryPersonId = deliveryPersonId;
                order.Status = OrderStatus.WaitingCourier; // معلق عند التوصيل

                await _orderRepository.UpdateAsync(order);

                // Send notifications
                await _notificationService.NotifyOrderStatusChangeAsync(orderId, previousStatus, OrderStatus.WaitingCourier, deliveryPersonId);

                _logger.LogInformation($"Order {orderId} successfully assigned to delivery person {deliveryPersonId} ({deliveryPerson.Name})");

                return new DeliveryAssignmentResultDto
                {
                    Success = true,
                    Message = "Order successfully assigned to delivery person",
                    AssignedDeliveryPersonId = deliveryPersonId,
                    AssignedDeliveryPersonName = deliveryPerson.Name,
                    AssignedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error manually assigning delivery person {deliveryPersonId} to order {orderId}");
                return new DeliveryAssignmentResultDto
                {
                    Success = false,
                    Message = "Internal error during assignment",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<bool> ReleaseOrderAssignmentAsync(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetAsync(orderId);
                if (order == null) return false;

                var previousStatus = order.Status;
                var previousDeliveryPersonId = order.DeliveryPersonId;

                // Release assignment
                order.DeliveryPersonId = null;
                order.Status = OrderStatus.ReadyForDelivery; // Back to ready for delivery

                await _orderRepository.UpdateAsync(order);

                // Send notifications
                if (previousDeliveryPersonId.HasValue)
                {
                    await _notificationService.NotifyOrderStatusChangeAsync(orderId, previousStatus, OrderStatus.ReadyForDelivery, previousDeliveryPersonId.Value);
                }

                _logger.LogInformation($"Order {orderId} assignment released from delivery person {previousDeliveryPersonId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error releasing order assignment for order {orderId}");
                return false;
            }
        }

        public async Task<List<OrderSummaryDto>> GetPendingDeliveryOrdersAsync(Guid? restaurantId = null)
        {
            try
            {
                var orders = await _orderRepository.GetListAsync(o => 
                    o.Status == OrderStatus.ReadyForDelivery && 
                    (restaurantId == null || o.RestaurantId == restaurantId));

                return ObjectMapper.Map<List<Order>, List<OrderSummaryDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending delivery orders");
                return new List<OrderSummaryDto>();
            }
        }

        public async Task<bool> UpdateDeliveryPersonLocationAsync(Guid deliveryPersonId, double latitude, double longitude, bool isAvailable)
        {
            try
            {
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                if (deliveryPerson == null) return false;

                // Update or create location
                if (deliveryPerson.CurrentLocation == null)
                {
                    deliveryPerson.CurrentLocation = new Location
                    {
                        Latitude = latitude,
                        Longitude = longitude
                    };
                }
                else
                {
                    deliveryPerson.CurrentLocation.Latitude = latitude;
                    deliveryPerson.CurrentLocation.Longitude = longitude;
                }

                // Update delivery status
                if (deliveryPerson.DeliveryStatus == null)
                {
                    deliveryPerson.DeliveryStatus = new DeliveryStatus
                    {
                        IsAvailable = isAvailable,
                        LastStatusUpdate = DateTime.UtcNow
                    };
                }
                else
                {
                    deliveryPerson.DeliveryStatus.IsAvailable = isAvailable;
                    deliveryPerson.DeliveryStatus.LastStatusUpdate = DateTime.UtcNow;
                }

                await _userRepository.UpdateAsync(deliveryPerson);

                _logger.LogDebug($"Updated location for delivery person {deliveryPersonId}: ({latitude}, {longitude}), Available: {isAvailable}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating delivery person location for {deliveryPersonId}");
                return false;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Calculate distance between two points using Haversine formula
        /// </summary>
        private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private async Task<bool> IsDeliveryPersonAvailable(Guid deliveryPersonId)
        {
            try
            {
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                return deliveryPerson?.DeliveryStatus?.IsAvailable ?? false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<int> GetActiveOrdersCount(Guid deliveryPersonId)
        {
            try
            {
                var activeOrders = await _orderRepository.GetListAsync(o => 
                    o.DeliveryPersonId == deliveryPersonId && 
                    (o.Status == OrderStatus.WaitingCourier || o.Status == OrderStatus.Delivering));

                return activeOrders.Count;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetCompletedDeliveriesCount(Guid deliveryPersonId)
        {
            try
            {
                var completedOrders = await _orderRepository.GetListAsync(o => 
                    o.DeliveryPersonId == deliveryPersonId && 
                    o.Status == OrderStatus.Delivered);

                return completedOrders.Count;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<double> GetDeliveryPersonRating(Guid deliveryPersonId)
        {
            try
            {
                // In a real implementation, you would calculate this from delivery reviews
                // For now, return a default rating
                await Task.CompletedTask;
                return 4.5; // Default rating
            }
            catch
            {
                return 0.0;
            }
        }

        #endregion
    }
}
