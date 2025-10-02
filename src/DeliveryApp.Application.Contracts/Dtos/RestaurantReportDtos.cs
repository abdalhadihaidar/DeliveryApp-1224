using System;
using System.Collections.Generic;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class MonthlyRestaurantReportDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime ReportGeneratedAt { get; set; }

        // Order Statistics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public double CompletionRate { get; set; } // Percentage
        public double CancellationRate { get; set; } // Percentage

        // Financial Summary
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal CommissionRate { get; set; }

        // Performance Metrics
        public double AveragePreparationTime { get; set; } // Minutes
        public double AverageOrderRating { get; set; }
        public int TotalCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public double CustomerRetentionRate { get; set; } // Percentage

        // Popular Items
        public List<PopularMenuItemDto> TopSellingItems { get; set; } = new List<PopularMenuItemDto>();

        // Daily Breakdown
        public List<DailyOrderSummaryDto> DailyBreakdown { get; set; } = new List<DailyOrderSummaryDto>();

        // Order Status Distribution
        public OrderStatusDistributionDto StatusDistribution { get; set; } = new OrderStatusDistributionDto();

        // Peak Hours Analysis
        public List<HourlyOrderVolumeDto> HourlyBreakdown { get; set; } = new List<HourlyOrderVolumeDto>();
    }

    public class PopularMenuItemDto
    {
        public Guid MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Price { get; set; }
        public double PopularityScore { get; set; } // Based on frequency and revenue
    }

    public class DailyOrderSummaryDto
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public double AveragePreparationTime { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    public class OrderStatusDistributionDto
    {
        public int PendingOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int ReadyForDeliveryOrders { get; set; }
        public int DeliveringOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    public class HourlyOrderVolumeDto
    {
        public int Hour { get; set; } // 0-23
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public double AveragePreparationTime { get; set; }
    }

    public class RestaurantPerformanceMetricsDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Time Performance
        public double AveragePreparationTime { get; set; } // Minutes
        public double AverageOrderToDeliveryTime { get; set; } // Minutes
        public double OnTimeDeliveryRate { get; set; } // Percentage
        
        // Quality Metrics
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public double CustomerSatisfactionScore { get; set; }

        // Efficiency Metrics
        public double OrderAcceptanceRate { get; set; } // Percentage
        public double OrderFulfillmentRate { get; set; } // Percentage
        public int PeakHourCapacity { get; set; }
        public double CapacityUtilization { get; set; } // Percentage
    }

    public class YearlyRestaurantSummaryDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public int Year { get; set; }

        // Annual Totals
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public decimal NetRevenue { get; set; }

        // Monthly Breakdown
        public List<MonthlyAggregateSummaryDto> MonthlyData { get; set; } = new List<MonthlyAggregateSummaryDto>();

        // Year-over-Year Growth
        public double RevenueGrowthRate { get; set; } // Percentage
        public double OrderGrowthRate { get; set; } // Percentage
        public double CustomerGrowthRate { get; set; } // Percentage
    }

    public class MonthlyAggregateSummaryDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public decimal NetRevenue { get; set; }
        public double AverageOrderValue { get; set; }
        public int UniqueCustomers { get; set; }
    }

    public class ReportRequestDto
    {
        public Guid RestaurantId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public bool IncludeDetailedBreakdown { get; set; } = true;
        public bool IncludePopularItems { get; set; } = true;
        public bool IncludePerformanceMetrics { get; set; } = true;
    }

    public class DateRangeReportRequestDto
    {
        public Guid RestaurantId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ReportType { get; set; } = "summary"; // summary, detailed, performance
    }

    public class CustomReportDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ReportType { get; set; } = string.Empty;

        // Flexible data container for different report types
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public List<string> Insights { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}
