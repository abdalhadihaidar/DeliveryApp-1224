using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Service for intelligent delivery assignment based on location and availability
    /// </summary>
    public interface IDeliveryAssignmentService : IApplicationService
    {
        /// <summary>
        /// Find and assign the nearest available delivery person to an order
        /// </summary>
        /// <param name="orderId">Order to assign</param>
        /// <param name="maxRadiusKm">Maximum search radius in kilometers (default: 10km)</param>
        Task<DeliveryAssignmentResultDto> AssignNearestDeliveryPersonAsync(Guid orderId, double maxRadiusKm = 10.0);
        
        /// <summary>
        /// Get all available delivery persons within radius of a location
        /// </summary>
        /// <param name="latitude">Target latitude</param>
        /// <param name="longitude">Target longitude</param>
        /// <param name="radiusKm">Search radius in kilometers</param>
        /// <param name="orderId">Optional order ID to check COD requirements</param>
        Task<List<AvailableDeliveryPersonDto>> GetAvailableDeliveryPersonsAsync(double latitude, double longitude, double radiusKm = 10.0, Guid? orderId = null);
        
        /// <summary>
        /// Manually assign a specific delivery person to an order
        /// </summary>
        /// <param name="orderId">Order to assign</param>
        /// <param name="deliveryPersonId">Delivery person to assign</param>
        Task<DeliveryAssignmentResultDto> ManualAssignDeliveryPersonAsync(Guid orderId, Guid deliveryPersonId);
        
        /// <summary>
        /// Release an order from delivery person (cancel assignment)
        /// </summary>
        /// <param name="orderId">Order to release</param>
        Task<bool> ReleaseOrderAssignmentAsync(Guid orderId);
        
        /// <summary>
        /// Get pending orders waiting for delivery assignment
        /// </summary>
        /// <param name="restaurantId">Optional: filter by restaurant</param>
        Task<List<OrderSummaryDto>> GetPendingDeliveryOrdersAsync(Guid? restaurantId = null);
        
        /// <summary>
        /// Update delivery person location and availability
        /// </summary>
        /// <param name="deliveryPersonId">Delivery person ID</param>
        /// <param name="latitude">Current latitude</param>
        /// <param name="longitude">Current longitude</param>
        /// <param name="isAvailable">Availability status</param>
        Task<bool> UpdateDeliveryPersonLocationAsync(Guid deliveryPersonId, double latitude, double longitude, bool isAvailable);
    }
}
