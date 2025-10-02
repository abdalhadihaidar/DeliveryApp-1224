using System;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Advanced delivery fee calculation service with dynamic pricing
    /// Supports distance-based, restaurant-specific, free delivery, rush delivery, and city-based pricing
    /// </summary>
    public class DeliveryFeeCalculationService : ApplicationService, IDeliveryFeeCalculationService, ITransientDependency
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DeliveryFeeCalculationService> _logger;

        public DeliveryFeeCalculationService(
            IRestaurantRepository restaurantRepository,
            IUserRepository userRepository,
            ILogger<DeliveryFeeCalculationService> logger)
        {
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Calculate delivery fee based on multiple factors
        /// </summary>
        public async Task<DeliveryFeeCalculationResultDto> CalculateDeliveryFeeAsync(DeliveryFeeCalculationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Calculating delivery fee for order {OrderId}", request.OrderId);

                // Get restaurant details
                var restaurant = await _restaurantRepository.GetWithAddressAsync(request.RestaurantId);
                if (restaurant?.Address == null)
                {
                    return new DeliveryFeeCalculationResultDto
                    {
                        Success = false,
                        Message = "Restaurant location not found",
                        ErrorCode = "RESTAURANT_LOCATION_MISSING"
                    };
                }

                // Get customer address details
                var customerAddress = await GetCustomerAddressAsync(request.CustomerAddressId);
                if (customerAddress == null)
                {
                    return new DeliveryFeeCalculationResultDto
                    {
                        Success = false,
                        Message = "Customer address not found",
                        ErrorCode = "CUSTOMER_ADDRESS_MISSING"
                    };
                }

                // Calculate distance
                var distance = CalculateDistanceKm(
                    restaurant.Address.Latitude, restaurant.Address.Longitude,
                    customerAddress.Latitude, customerAddress.Longitude);

                // Convert domain address to DTO for city type determination
                var restaurantAddressDto = new AddressDto
                {
                    Id = restaurant.Address.Id,
                    Latitude = restaurant.Address.Latitude,
                    Longitude = restaurant.Address.Longitude,
                    City = restaurant.Address.City
                };

                // Determine city type
                var cityType = DetermineCityType(restaurantAddressDto, customerAddress);

                // Get system settings (in real implementation, get from settings service)
                var settings = GetSystemDeliverySettings();

                // Calculate base fee
                var baseFee = CalculateBaseFee(restaurant, settings, cityType);

                // Apply free delivery logic
                if (ShouldApplyFreeDelivery(request.OrderAmount, settings))
                {
                    return new DeliveryFeeCalculationResultDto
                    {
                        Success = true,
                        DeliveryFee = 0.0m,
                        BaseFee = baseFee,
                        DistanceKm = distance,
                        CityType = cityType,
                        IsFreeDelivery = true,
                        FreeDeliveryReason = $"Order amount {request.OrderAmount:C} exceeds free delivery threshold {settings.FreeDeliveryThreshold:C}",
                        CalculationBreakdown = new DeliveryFeeBreakdownDto
                        {
                            BaseFee = baseFee,
                            DistanceFee = 0.0m,
                            RushDeliveryFee = 0.0m,
                            CityTypeMultiplier = 1.0m,
                            FreeDeliveryDiscount = baseFee,
                            FinalFee = 0.0m
                        }
                    };
                }

                // Apply rush delivery logic
                var rushDeliveryFee = 0.0m;
                if (request.IsRushDelivery)
                {
                    rushDeliveryFee = (decimal)settings.RushDeliveryFee;
                }

                // Calculate distance-based fee
                var distanceFee = CalculateDistanceFee(distance, settings, cityType);

                // Calculate city type multiplier
                var cityTypeMultiplier = GetCityTypeMultiplier(cityType, settings);

                // Calculate final fee
                var finalFee = (baseFee + distanceFee + rushDeliveryFee) * cityTypeMultiplier;

                // Apply minimum order validation
                if (request.OrderAmount < (decimal)settings.MinimumOrderAmount)
                {
                    return new DeliveryFeeCalculationResultDto
                    {
                        Success = false,
                        Message = $"Minimum order amount is {settings.MinimumOrderAmount:C}",
                        ErrorCode = "MINIMUM_ORDER_NOT_MET"
                    };
                }

                // Validate maximum delivery distance
                if (distance > settings.MaxDeliveryDistance)
                {
                    return new DeliveryFeeCalculationResultDto
                    {
                        Success = false,
                        Message = $"Delivery distance {distance:F2}km exceeds maximum allowed distance {settings.MaxDeliveryDistance}km",
                        ErrorCode = "DELIVERY_DISTANCE_EXCEEDED"
                    };
                }

                var result = new DeliveryFeeCalculationResultDto
                {
                    Success = true,
                    DeliveryFee = finalFee,
                    BaseFee = baseFee,
                    DistanceKm = distance,
                    CityType = cityType,
                    IsFreeDelivery = false,
                    IsRushDelivery = request.IsRushDelivery,
                    CalculationBreakdown = new DeliveryFeeBreakdownDto
                    {
                        BaseFee = baseFee,
                        DistanceFee = distanceFee,
                        RushDeliveryFee = rushDeliveryFee,
                        CityTypeMultiplier = cityTypeMultiplier,
                        FreeDeliveryDiscount = 0.0m,
                        FinalFee = finalFee
                    }
                };

                _logger.LogInformation("Delivery fee calculated: {Fee:C} for order {OrderId}", finalFee, request.OrderId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating delivery fee for order {OrderId}", request.OrderId);
                return new DeliveryFeeCalculationResultDto
                {
                    Success = false,
                    Message = "Internal error during fee calculation",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Get available delivery fee options for a location
        /// </summary>
        public async Task<DeliveryFeeOptionsDto> GetDeliveryFeeOptionsAsync(Guid restaurantId, Guid customerAddressId)
        {
            try
            {
                var restaurant = await _restaurantRepository.GetWithAddressAsync(restaurantId);
                var customerAddress = await GetCustomerAddressAsync(customerAddressId);

                if (restaurant?.Address == null || customerAddress == null)
                {
                    return new DeliveryFeeOptionsDto { Success = false };
                }

                var distance = CalculateDistanceKm(
                    restaurant.Address.Latitude, restaurant.Address.Longitude,
                    customerAddress.Latitude, customerAddress.Longitude);

                // Convert domain address to DTO for city type determination
                var restaurantAddressDto = new AddressDto
                {
                    Id = restaurant.Address.Id,
                    Latitude = restaurant.Address.Latitude,
                    Longitude = restaurant.Address.Longitude,
                    City = restaurant.Address.City
                };

                var cityType = DetermineCityType(restaurantAddressDto, customerAddress);
                var settings = GetSystemDeliverySettings();

                return new DeliveryFeeOptionsDto
                {
                    Success = true,
                    StandardDelivery = new DeliveryFeeOptionDto
                    {
                        Type = DeliveryType.Standard,
                        EstimatedTime = 45,
                        Fee = CalculateBaseFee(restaurant, settings, cityType) * GetCityTypeMultiplier(cityType, settings)
                    },
                    RushDelivery = new DeliveryFeeOptionDto
                    {
                        Type = DeliveryType.Rush,
                        EstimatedTime = settings.RushDeliveryTime,
                        Fee = (CalculateBaseFee(restaurant, settings, cityType) + (decimal)settings.RushDeliveryFee) * GetCityTypeMultiplier(cityType, settings)
                    },
                    FreeDeliveryThreshold = settings.FreeDeliveryThreshold,
                    MinimumOrderAmount = settings.MinimumOrderAmount,
                    MaxDeliveryDistance = settings.MaxDeliveryDistance,
                    CityType = cityType,
                    DistanceKm = distance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery fee options");
                return new DeliveryFeeOptionsDto { Success = false };
            }
        }

        #region Private Methods

        private async Task<AddressDto> GetCustomerAddressAsync(Guid addressId)
        {
            // In real implementation, get from address repository
            // For now, return a mock address
            return new AddressDto
            {
                Id = addressId,
                Latitude = 33.5138, // Damascus coordinates
                Longitude = 36.2765,
                City = "Damascus"
            };
        }

        private DeliveryCityType DetermineCityType(AddressDto restaurantAddress, AddressDto customerAddress)
        {
            // Calculate distance between restaurant and customer
            var distance = CalculateDistanceKm(
                restaurantAddress.Latitude, restaurantAddress.Longitude,
                customerAddress.Latitude, customerAddress.Longitude);

            // Get system settings for in-town distance threshold
            var settings = GetSystemDeliverySettings();
            
            // If distance is within in-town threshold, it's in-town
            // Otherwise, it's out-of-town and will be auto-calculated based on distance
            if (distance <= settings.InTownDistanceThreshold)
            {
                return DeliveryCityType.InTown;
            }
            return DeliveryCityType.OutOfTown;
        }

        private decimal CalculateBaseFee(Domain.Entities.Restaurant restaurant, DeliverySettingsDto settings, DeliveryCityType cityType)
        {
            // Use restaurant-specific fee if available, otherwise use city-specific system default
            decimal baseFee;
            
            if (restaurant.DeliveryFee > 0)
            {
                // Restaurant has custom fee - apply city type multiplier
                var cityMultiplier = cityType == DeliveryCityType.InTown ? 1.0m : 1.5m;
                baseFee = restaurant.DeliveryFee * cityMultiplier;
            }
            else
            {
                // Use system city-specific base fees
                baseFee = cityType == DeliveryCityType.InTown 
                    ? (decimal)settings.InTownBaseFee 
                    : (decimal)settings.OutOfTownBaseFee;
            }
            
            return baseFee;
        }

        private decimal CalculateDistanceFee(double distance, DeliverySettingsDto settings, DeliveryCityType cityType)
        {
            var distanceFee = 0.0m;
            
            if (cityType == DeliveryCityType.InTown)
            {
                // In-town: Fixed fee for any distance within threshold
                // No additional distance charges within in-town area
                distanceFee = 0.0m;
            }
            else
            {
                // Out-of-town: Auto-calculated based on actual distance
                // Charge for every km beyond the in-town threshold
                var outOfTownKm = distance - settings.InTownDistanceThreshold;
                if (outOfTownKm > 0)
                {
                    var ratePerKm = (decimal)settings.OutOfTownRatePerKm;
                    distanceFee = (decimal)outOfTownKm * ratePerKm;
                }
            }
            
            return distanceFee;
        }

        private decimal GetCityTypeMultiplier(DeliveryCityType cityType, DeliverySettingsDto settings)
        {
            // Since we now use different base fees for in-town vs out-of-town,
            // the multiplier is always 1.0 (the base fee already accounts for city type)
            return 1.0m;
        }

        private bool ShouldApplyFreeDelivery(decimal orderAmount, DeliverySettingsDto settings)
        {
            return orderAmount >= (decimal)settings.FreeDeliveryThreshold;
        }

        private DeliverySettingsDto GetSystemDeliverySettings()
        {
            // In real implementation, get from settings service
            return new DeliverySettingsDto
            {
                DeliveryFee = 5.0,
                FreeDeliveryThreshold = 50.0,
                RushDeliveryFee = 10.0,
                RushDeliveryTime = 30,
                MinimumOrderAmount = 20.0,
                MaxDeliveryDistance = 25.0,
                
                // City-based delivery settings
                InTownDistanceThreshold = 5.0, // km - within this distance is in-town
                OutOfTownRatePerKm = 2.0, // Rate per km for out-of-town
                InTownBaseFee = 5.0, // Fixed fee for in-town
                OutOfTownBaseFee = 8.0 // Base fee for out-of-town
            };
        }

        private double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        #endregion
    }
}
