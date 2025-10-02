using System;
using System.Collections.Generic;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class DeliveryAssignmentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? AssignedDeliveryPersonId { get; set; }
        public string? AssignedDeliveryPersonName { get; set; }
        public double? DistanceKm { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class AvailableDeliveryPersonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceKm { get; set; }
        public bool IsAvailable { get; set; }
        public int CurrentActiveOrders { get; set; }
        public DateTime LastLocationUpdate { get; set; }
        public double Rating { get; set; }
        public int CompletedDeliveries { get; set; }
        
        // COD-related fields
        public bool AcceptsCOD { get; set; }
        public decimal CashBalance { get; set; }
        public decimal MaxCashLimit { get; set; }
        public bool HasSufficientCashForOrder { get; set; }
        public decimal OrderAmount { get; set; }
    }

    public class DeliveryLocationUpdateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class DeliveryAssignmentRequestDto
    {
        public Guid OrderId { get; set; }
        public Guid? PreferredDeliveryPersonId { get; set; }
        public double MaxRadiusKm { get; set; } = 10.0;
        public bool AutoAssign { get; set; } = true;
    }

    public class DeliveryPersonLocationDto
    {
        public Guid DeliveryPersonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime LastLocationUpdate { get; set; }
        public int CurrentOrderCount { get; set; }
    }

    public class DeliveryZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public double RadiusKm { get; set; }
        public bool IsActive { get; set; }
        public int AvailableDeliveryPersons { get; set; }
        public int PendingOrders { get; set; }
    }

    public class NearestDeliveryPersonsRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusKm { get; set; } = 10.0;
        public int MaxResults { get; set; } = 10;
        public bool OnlyAvailable { get; set; } = true;
    }
}
