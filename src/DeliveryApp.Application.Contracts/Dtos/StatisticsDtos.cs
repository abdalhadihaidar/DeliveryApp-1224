using System;
using System.Collections.Generic;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class DeliveryStatisticsDto
    {
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public decimal TotalEarnings { get; set; }
        public double AverageRating { get; set; }
        public int AverageDeliveryTime { get; set; } // in minutes
    }

    public class RestaurantStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<MenuItemSalesDto> TopSellingItems { get; set; }
        
        public RestaurantStatisticsDto()
        {
            TopSellingItems = new List<MenuItemSalesDto>();
        }
    }

    public class MenuItemSalesDto
    {
        public Guid MenuItemId { get; set; }
        public string? Name { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}
