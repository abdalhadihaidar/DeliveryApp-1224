using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    // Enhanced Restaurant Owner Statistics DTO
    public class RestaurantOwnerStatisticsDto
    {
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public DateTime GeneratedAt { get; set; }
        
        // Basic Statistics
        public int TotalRestaurants { get; set; }
        public int ActiveRestaurants { get; set; }
        public int PendingRestaurants { get; set; }
        
        // Financial Statistics
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        
        // Order Statistics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public double CompletionRate { get; set; }
        public double CancellationRate { get; set; }
        
        // Performance Metrics
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int PositiveReviews { get; set; }
        public int NegativeReviews { get; set; }
        
        // Time-based Statistics
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int DaysSinceLastLogin { get; set; }
        public int DaysSinceLastOrder { get; set; }
        
        // Monthly Trends
        public List<MonthlyOwnerStatsDto> MonthlyTrends { get; set; } = new List<MonthlyOwnerStatsDto>();
        
        // Top Performing Restaurants
        public List<RestaurantPerformanceSummaryDto> TopRestaurants { get; set; } = new List<RestaurantPerformanceSummaryDto>();
    }

    // Monthly Statistics for Owner
    public class MonthlyOwnerStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public int NewRestaurants { get; set; }
        public double AverageRating { get; set; }
    }

    // Restaurant Performance Summary
    public class RestaurantPerformanceSummaryDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
    }

    // Restaurant Owner Performance DTO
    public class RestaurantOwnerPerformanceDto
    {
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        // Performance Metrics
        public decimal TotalRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int TotalOrders { get; set; }
        public int OrderGrowth { get; set; }
        public double AverageRating { get; set; }
        public double RatingImprovement { get; set; }
        
        // Restaurant Performance
        public List<RestaurantPerformanceDto> RestaurantPerformance { get; set; } = new List<RestaurantPerformanceDto>();
        
        // Top Selling Items
        public List<PopularMenuItemDto> TopSellingItems { get; set; } = new List<PopularMenuItemDto>();
        
        // Customer Analysis
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public double CustomerRetentionRate { get; set; }
        
        // Financial Analysis
        public decimal TotalCommission { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    // Individual Restaurant Performance
    public class RestaurantPerformanceDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public decimal CommissionPaid { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
    }

    // Restaurant Owner Dashboard DTO
    public class RestaurantOwnerDashboardDto
    {
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        
        // Quick Stats
        public int TotalRestaurants { get; set; }
        public int ActiveRestaurants { get; set; }
        public int PendingRestaurants { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int MonthlyOrders { get; set; }
        
        // Recent Activity
        public List<DashboardRecentActivityDto> RecentActivities { get; set; } = new List<DashboardRecentActivityDto>();
        
        // Pending Actions
        public List<PendingActionDto> PendingActions { get; set; } = new List<PendingActionDto>();
        
        // Performance Summary
        public OwnerPerformanceSummaryDto PerformanceSummary { get; set; }
        
        // Restaurant List
        public List<RestaurantSummaryDto> Restaurants { get; set; } = new List<RestaurantSummaryDto>();
    }

    // Recent Activity DTO
   /* public class RecentActivityDto
    {
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string RestaurantName { get; set; }
        public string Icon { get; set; }
    }*/

    // Pending Action DTO
    public class PendingActionDto
    {
        public string ActionType { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RestaurantName { get; set; }
    }

    // Owner Performance Summary
    public class OwnerPerformanceSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int OrderGrowth { get; set; }
        public double CustomerSatisfaction { get; set; }
        public string PerformanceGrade { get; set; }
    }

    // Bulk Operation DTOs
    public class BulkOwnerOperationDto
    {
        public List<Guid> OwnerIds { get; set; } = new List<Guid>();
        public string OperationType { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
    }

    public class BulkOperationResultDto
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> SuccessMessages { get; set; } = new List<string>();
    }

    // Owner Search and Filter DTOs
    public class OwnerSearchDto
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 100;
    }

    // Owner Export DTO
    public class OwnerExportDto
    {
        public List<RestaurantOwnerDto> Owners { get; set; } = new List<RestaurantOwnerDto>();
        public DateTime ExportDate { get; set; }
        public string ExportFormat { get; set; }
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
    }

    // Owner Analytics DTO
    public class OwnerAnalyticsDto
    {
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        // Revenue Analytics
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new List<DailyRevenueDto>();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new List<MonthlyRevenueDto>();
        
        // Order Analytics
        public List<DailyOrderDto> DailyOrders { get; set; } = new List<DailyOrderDto>();
        public List<HourlyOrderDto> HourlyOrders { get; set; } = new List<HourlyOrderDto>();
        
        // Customer Analytics
        public List<CustomerSegmentDto> CustomerSegments { get; set; } = new List<CustomerSegmentDto>();
        
        // Performance Trends
        public List<PerformanceTrendDto> PerformanceTrends { get; set; } = new List<PerformanceTrendDto>();
    }

    // Analytics Detail DTOs

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class DailyOrderDto
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public double CompletionRate { get; set; }
    }

    public class HourlyOrderDto
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomerSegmentDto
    {
        public string SegmentName { get; set; }
        public int CustomerCount { get; set; }
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }

    public class PerformanceTrendDto
    {
        public DateTime Date { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    // Restaurant Owner Admin DTO
    public class RestaurantOwnerAdminDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string UserType { get; set; }
        public int ManagedRestaurants { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Status { get; set; }
        public string VerificationStatus { get; set; }
        public bool ProfileComplete { get; set; }
        public List<RestaurantDto> Restaurants { get; set; } = new List<RestaurantDto>();
    }

    // Admin Restaurant Owner Statistics DTO
    public class AdminRestaurantOwnerStatisticsDto
    {
        public int TotalOwners { get; set; }
        public int ActiveOwners { get; set; }
        public int PendingApprovals { get; set; }
        public int InactiveOwners { get; set; }
        public int TotalRestaurants { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public List<RestaurantOwnerAdminDto> TopPerformingOwners { get; set; } = new List<RestaurantOwnerAdminDto>();
    }

    // Additional DTOs for restaurant owner management
    public class CreateRestaurantOwnerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "restaurant_owner"; // Default to restaurant_owner role

        // Additional restaurant owner specific properties
        public bool IsAdminApproved { get; set; } = false;
        public string? ReviewReason { get; set; }
    }

    public class UpdateRestaurantOwnerDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }

        // Additional restaurant owner specific properties
        public bool? IsAdminApproved { get; set; }
        public string? ReviewReason { get; set; }
        public bool? IsActive { get; set; }
    }
}
