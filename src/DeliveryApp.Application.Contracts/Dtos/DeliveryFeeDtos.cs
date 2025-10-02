using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Request DTO for delivery fee calculation
    /// </summary>
    public class DeliveryFeeCalculationRequestDto
    {
        public Guid OrderId { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid CustomerAddressId { get; set; }
        
        [Range(0, 10000)]
        public decimal OrderAmount { get; set; }
        
        public bool IsRushDelivery { get; set; } = false;
        public DateTime? PreferredDeliveryTime { get; set; }
    }

    /// <summary>
    /// Result DTO for delivery fee calculation
    /// </summary>
    public class DeliveryFeeCalculationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        
        [Range(0, 1000)]
        public decimal DeliveryFee { get; set; }
        
        [Range(0, 1000)]
        public decimal BaseFee { get; set; }
        
        public double DistanceKm { get; set; }
        public DeliveryCityType CityType { get; set; }
        public bool IsFreeDelivery { get; set; }
        public bool IsRushDelivery { get; set; }
        public string FreeDeliveryReason { get; set; } = string.Empty;
        public DeliveryFeeBreakdownDto CalculationBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Breakdown of delivery fee calculation
    /// </summary>
    public class DeliveryFeeBreakdownDto
    {
        [Range(0, 1000)]
        public decimal BaseFee { get; set; }
        
        [Range(0, 1000)]
        public decimal DistanceFee { get; set; }
        
        [Range(0, 1000)]
        public decimal RushDeliveryFee { get; set; }
        
        [Range(0, 10)]
        public decimal CityTypeMultiplier { get; set; }
        
        [Range(0, 1000)]
        public decimal FreeDeliveryDiscount { get; set; }
        
        [Range(0, 1000)]
        public decimal FinalFee { get; set; }
    }

    /// <summary>
    /// Available delivery fee options
    /// </summary>
    public class DeliveryFeeOptionsDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DeliveryFeeOptionDto StandardDelivery { get; set; } = new();
        public DeliveryFeeOptionDto RushDelivery { get; set; } = new();
        public double FreeDeliveryThreshold { get; set; }
        public double MinimumOrderAmount { get; set; }
        public double MaxDeliveryDistance { get; set; }
        public DeliveryCityType CityType { get; set; }
        public double DistanceKm { get; set; }
    }

    /// <summary>
    /// Individual delivery fee option
    /// </summary>
    public class DeliveryFeeOptionDto
    {
        public DeliveryType Type { get; set; }
        public int EstimatedTime { get; set; } // in minutes
        
        [Range(0, 1000)]
        public decimal Fee { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Delivery city type enumeration
    /// </summary>
    public enum DeliveryCityType
    {
        InTown = 0,      // Within the same city
        OutOfTown = 1    // Different city or outside city limits
    }

    /// <summary>
    /// Delivery type enumeration
    /// </summary>
    public enum DeliveryType
    {
        Standard = 0,    // Regular delivery
        Rush = 1         // Rush/express delivery
    }

    /// <summary>
    /// Enhanced address DTO with city information
  /*  /// </summary>
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsInTown { get; set; } = true;
        public string Landmarks { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
    }*/
}
