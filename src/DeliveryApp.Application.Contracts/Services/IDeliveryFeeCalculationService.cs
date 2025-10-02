using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Service for advanced delivery fee calculation with dynamic pricing
    /// </summary>
    public interface IDeliveryFeeCalculationService : IApplicationService
    {
        /// <summary>
        /// Calculate delivery fee based on multiple factors
        /// </summary>
        /// <param name="request">Delivery fee calculation request</param>
        /// <returns>Delivery fee calculation result</returns>
        Task<DeliveryFeeCalculationResultDto> CalculateDeliveryFeeAsync(DeliveryFeeCalculationRequestDto request);

        /// <summary>
        /// Get available delivery fee options for a location
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="customerAddressId">Customer address ID</param>
        /// <returns>Available delivery fee options</returns>
        Task<DeliveryFeeOptionsDto> GetDeliveryFeeOptionsAsync(Guid restaurantId, Guid customerAddressId);
    }
}
