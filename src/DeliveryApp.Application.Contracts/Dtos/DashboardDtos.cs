using System;
using System.Collections.Generic;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Dtos
{
    // Main Dashboard Overview DTO
    public class DashboardOverviewDto
    {
        public SalesSummary Sales { get; set; }
        public DeliverySummary Deliveries { get; set; }
        public CustomerSummary Customers { get; set; }
        public StoreSummary Stores { get; set; }
        public ReviewSummary Reviews { get; set; }
    }

    // Summary DTOs for Dashboard Overview
    public class SalesSummary
    {
        public decimal TotalAmount { get; set; }
        public decimal TrendPercentage { get; set; }
    }

    public class DeliverySummary
    {
        public int CurrentDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int CancelledDeliveries { get; set; }
        public TimeSpan AverageDeliveryTime { get; set; }
    }

    public class CustomerSummary
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
    }

    public class StoreSummary
    {
        public int TotalStores { get; set; }
        public int ActiveStores { get; set; }
        public decimal TotalSalesAmount { get; set; }
    }

    public class ReviewSummary
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int PositiveReviews { get; set; }
        public int NegativeReviews { get; set; }
    }

    // DTO for Reviews
    public class DashboardReviewDto
    {
        public Guid Id { get; set; }
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string StoreName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
    }

    // DTO for Current Deliveries
    public class CurrentDeliveryDto
    {
        public string Id { get; set; }
        public DateTime OrderTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string StoreName { get; set; }
        public string City { get; set; }
        public decimal Amount { get; set; }
        public string DeliveryTimeDifference { get; set; }
        public string CustomerStatus { get; set; }
        public string OrderStatus { get; set; }
        public int? EstimatedDeliveryTime { get; set; }
        public string? DeliveryPersonId { get; set; }
        public string? DeliveryPersonName { get; set; }
        public string? ActualDeliveryTime { get; set; }
    }

    // DTO for Customers
    public class DashboardCustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string InteractionStatus { get; set; }
        public int TotalOrders { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    // DTO for Cancelled Orders
    public class CancelledOrderDto
    {
        public string Id { get; set; }
        public DateTime OrderTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string StoreName { get; set; }
        public string City { get; set; }
        public decimal Amount { get; set; }
        public string CustomerStatus { get; set; }
        public string CancellationReason { get; set; }
    }

    // DTO for Completed Orders
    public class CompletedOrderDto
    {
        public string Id { get; set; }
        public DateTime OrderTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string StoreName { get; set; }
        public string City { get; set; }
        public decimal Amount { get; set; }
        public string DeliveryTimeDifference { get; set; }
        public string CustomerStatus { get; set; }
    }

    // DTO for Time Difference Analysis
    public class TimeDifferenceDto
    {
        public string OrderId { get; set; }
        public string DeliveryGuyName { get; set; }
        public int ExpectedTimeMinutes { get; set; }
        public int ActualTimeMinutes { get; set; }
        public string TimeDifference { get; set; }
        public string Note { get; set; }
    }

    // DTO for Stores
    public class StoreDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public string Evaluation { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal RequestedAmount { get; set; }
        public DateTime JoinDate { get; set; }
        public string PaymentStatus { get; set; }
        public string StoreStatus { get; set; }
    }


    // Previous Period Data DTO
    public class PreviousPeriodDataDto
    {
        public int TotalDeliveries { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public double AverageRating { get; set; }
        public int NewCustomers { get; set; }
        public TimeSpan AverageDeliveryTime { get; set; }
        public string Period { get; set; } // "last_week", "last_month", "last_quarter"
    }

    // Recent Activity DTO
    public class DashboardRecentActivityDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } // "order_created", "order_delivered", "order_cancelled", "review_added", "customer_registered"
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; } // "customer", "admin", "delivery_person", "restaurant_owner"
        public Guid? RelatedEntityId { get; set; } // Order ID, Review ID, etc.
        public string RelatedEntityType { get; set; } // "order", "review", "customer"
    }

}


