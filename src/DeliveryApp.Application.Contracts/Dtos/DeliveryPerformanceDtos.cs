using System;
using System.Collections.Generic;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Delivery person performance data for admin dashboard
    /// </summary>
    public class DeliveryPersonPerformanceDto
    {
        public string DeliveryPersonId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CompletedOrders { get; set; }
        public double AverageDeliveryMinutes { get; set; }
        public double Rating { get; set; }
        public decimal TotalEarnings { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
        public double OnTimePercentage { get; set; }
        public DateTime LastDeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty; // Active, Inactive, etc.
    }

    /// <summary>
    /// Detailed performance metrics for a specific delivery person
    /// </summary>
    public class DetailedDeliveryPerformanceDto
    {
        public string DeliveryPersonId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Order Statistics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public double CompletionRate { get; set; }

        // Time Performance
        public double AverageDeliveryMinutes { get; set; }
        public double FastestDeliveryMinutes { get; set; }
        public double SlowestDeliveryMinutes { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
        public double OnTimePercentage { get; set; }

        // Quality Metrics
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int FiveStarRatings { get; set; }
        public int FourStarRatings { get; set; }
        public int ThreeStarRatings { get; set; }
        public int TwoStarRatings { get; set; }
        public int OneStarRatings { get; set; }

        // Financial Metrics
        public decimal TotalEarnings { get; set; }
        public decimal AverageEarningsPerDelivery { get; set; }
        public decimal TotalTips { get; set; }

        // Geographic Performance
        public List<DeliveryZonePerformanceDto> ZonePerformance { get; set; } = new();

        // Daily Performance Trend
        public List<DailyPerformanceDto> DailyPerformance { get; set; } = new();
    }

    /// <summary>
    /// Performance summary statistics for all delivery persons
    /// </summary>
    public class DeliveryPerformanceSummaryDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Overall Statistics
        public int TotalDeliveryPersons { get; set; }
        public int ActiveDeliveryPersons { get; set; }
        public int TotalDeliveries { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double AverageRating { get; set; }

        // Performance Distribution
        public int ExcellentPerformers { get; set; } // Rating >= 4.5
        public int GoodPerformers { get; set; } // Rating 3.5-4.4
        public int AveragePerformers { get; set; } // Rating 2.5-3.4
        public int PoorPerformers { get; set; } // Rating < 2.5

        // Time Performance
        public double AverageOnTimePercentage { get; set; }
        public int TotalOnTimeDeliveries { get; set; }
        public int TotalLateDeliveries { get; set; }

        // Top Performers
        public List<TopPerformerDto> TopPerformers { get; set; } = new();
    }

    /// <summary>
    /// Delivery zone performance data
    /// </summary>
    public class DeliveryZonePerformanceDto
    {
        public string ZoneName { get; set; } = string.Empty;
        public int DeliveryCount { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalEarnings { get; set; }
    }

    /// <summary>
    /// Daily performance data
    /// </summary>
    public class DailyPerformanceDto
    {
        public DateTime Date { get; set; }
        public int DeliveryCount { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalEarnings { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
    }

    /// <summary>
    /// Top performer data
    /// </summary>
    public class TopPerformerDto
    {
        public string DeliveryPersonId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CompletedOrders { get; set; }
        public double Rating { get; set; }
        public double AverageDeliveryTime { get; set; }
        public decimal TotalEarnings { get; set; }
        public string PerformanceCategory { get; set; } = string.Empty; // "Most Orders", "Best Rating", "Fastest Delivery", etc.
    }
}
