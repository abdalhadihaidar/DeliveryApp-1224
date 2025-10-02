using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Comprehensive restaurant reporting service with financial tracking and performance analysis
    /// </summary>
    public class RestaurantReportService : ApplicationService, IRestaurantReportService, ITransientDependency
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IRepository<MenuItem, Guid> _menuItemRepository;
        private readonly IRepository<OrderItem, Guid> _orderItemRepository;
        private readonly IRepository<Review, Guid> _reviewRepository;
        private readonly ILogger<RestaurantReportService> _logger;

        public RestaurantReportService(
            IOrderRepository orderRepository,
            IRestaurantRepository restaurantRepository,
            IRepository<MenuItem, Guid> menuItemRepository,
            IRepository<OrderItem, Guid> orderItemRepository,
            IRepository<Review, Guid> reviewRepository,
            ILogger<RestaurantReportService> logger)
        {
            _orderRepository = orderRepository;
            _restaurantRepository = restaurantRepository;
            _menuItemRepository = menuItemRepository;
            _orderItemRepository = orderItemRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<MonthlyRestaurantReportDto> GenerateMonthlyReportAsync(ReportRequestDto request)
        {
            try
            {
                var restaurant = await _restaurantRepository.GetAsync(request.RestaurantId);
                var fromDate = new DateTime(request.Year, request.Month, 1);
                var toDate = fromDate.AddMonths(1).AddDays(-1);

                _logger.LogInformation($"Generating monthly report for restaurant {restaurant.Name} for {request.Month}/{request.Year}");

                // Get all orders for the month
                var orders = await _orderRepository.GetListAsync(o => 
                    o.RestaurantId == request.RestaurantId &&
                    o.OrderDate >= fromDate &&
                    o.OrderDate <= toDate);

                var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
                var cancelledOrders = orders.Where(o => o.Status == OrderStatus.Cancelled).ToList();

                // Calculate basic metrics
                var totalOrders = orders.Count;
                var completedCount = completedOrders.Count;
                var cancelledCount = cancelledOrders.Count;
                var completionRate = totalOrders > 0 ? (double)completedCount / totalOrders * 100 : 0;
                var cancellationRate = totalOrders > 0 ? (double)cancelledCount / totalOrders * 100 : 0;

                // Financial calculations
                var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
                var commissionRate = restaurant.CommissionRate ?? 0.15m;
                var totalCommission = totalRevenue * commissionRate;
                var netRevenue = totalRevenue - totalCommission;
                var averageOrderValue = completedCount > 0 ? totalRevenue / completedCount : 0;

                // Performance metrics
                var averagePreparationTime = await CalculateAveragePreparationTimeAsync(completedOrders);
                var averageRating = await CalculateAverageRatingAsync(request.RestaurantId, fromDate, toDate);
                
                // Customer metrics
                var uniqueCustomers = orders.Select(o => o.UserId).Distinct().Count();
                var returningCustomers = await CalculateReturningCustomersAsync(request.RestaurantId, fromDate, toDate);
                var retentionRate = uniqueCustomers > 0 ? (double)returningCustomers / uniqueCustomers * 100 : 0;

                var report = new MonthlyRestaurantReportDto
                {
                    RestaurantId = request.RestaurantId,
                    RestaurantName = restaurant.Name,
                    Year = request.Year,
                    Month = request.Month,
                    ReportGeneratedAt = DateTime.UtcNow,

                    // Order Statistics
                    TotalOrders = totalOrders,
                    CompletedOrders = completedCount,
                    CancelledOrders = cancelledCount,
                    CompletionRate = Math.Round(completionRate, 2),
                    CancellationRate = Math.Round(cancellationRate, 2),

                    // Financial Summary
                    TotalRevenue = totalRevenue,
                    TotalCommissionPaid = totalCommission,
                    NetRevenue = netRevenue,
                    AverageOrderValue = Math.Round(averageOrderValue, 2),
                    CommissionRate = commissionRate * 100, // Convert to percentage

                    // Performance Metrics
                    AveragePreparationTime = Math.Round(averagePreparationTime, 2),
                    AverageOrderRating = Math.Round(averageRating, 2),
                    TotalCustomers = uniqueCustomers,
                    ReturningCustomers = returningCustomers,
                    CustomerRetentionRate = Math.Round(retentionRate, 2)
                };

                // Add detailed breakdowns if requested
                if (request.IncludePopularItems)
                {
                    report.TopSellingItems = await GetTopSellingItemsAsync(request.RestaurantId, fromDate, toDate, 10);
                }

                if (request.IncludeDetailedBreakdown)
                {
                    report.DailyBreakdown = await GetDailyBreakdownAsync(request.RestaurantId, fromDate, toDate);
                    report.HourlyBreakdown = await GetHourlyOrderVolumeAsync(request.RestaurantId, fromDate, toDate);
                }

                report.StatusDistribution = CalculateStatusDistribution(orders);

                _logger.LogInformation($"Successfully generated monthly report for restaurant {restaurant.Name}");
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating monthly report for restaurant {request.RestaurantId}");
                throw;
            }
        }

        public async Task<RestaurantPerformanceMetricsDto> GetPerformanceMetricsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                var orders = await _orderRepository.GetListAsync(o => 
                    o.RestaurantId == restaurantId &&
                    o.OrderDate >= fromDate &&
                    o.OrderDate <= toDate);

                var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();

                // Time performance calculations
                var averagePreparationTime = await CalculateAveragePreparationTimeAsync(completedOrders);
                var averageOrderToDeliveryTime = await CalculateAverageOrderToDeliveryTimeAsync(completedOrders);
                var onTimeDeliveryRate = await CalculateOnTimeDeliveryRateAsync(completedOrders);

                // Quality metrics
                var averageRating = await CalculateAverageRatingAsync(restaurantId, fromDate, toDate);
                var totalReviews = await GetReviewCountAsync(restaurantId, fromDate, toDate);
                var customerSatisfactionScore = await CalculateCustomerSatisfactionScoreAsync(restaurantId, fromDate, toDate);

                // Efficiency metrics
                var orderAcceptanceRate = CalculateOrderAcceptanceRate(orders);
                var orderFulfillmentRate = CalculateOrderFulfillmentRate(orders);
                var peakHourCapacity = await CalculatePeakHourCapacityAsync(orders);
                var capacityUtilization = await CalculateCapacityUtilizationAsync(orders, peakHourCapacity);

                return new RestaurantPerformanceMetricsDto
                {
                    RestaurantId = restaurantId,
                    RestaurantName = restaurant.Name,
                    FromDate = fromDate,
                    ToDate = toDate,

                    // Time Performance
                    AveragePreparationTime = Math.Round(averagePreparationTime, 2),
                    AverageOrderToDeliveryTime = Math.Round(averageOrderToDeliveryTime, 2),
                    OnTimeDeliveryRate = Math.Round(onTimeDeliveryRate, 2),

                    // Quality Metrics
                    AverageRating = Math.Round(averageRating, 2),
                    TotalReviews = totalReviews,
                    CustomerSatisfactionScore = Math.Round(customerSatisfactionScore, 2),

                    // Efficiency Metrics
                    OrderAcceptanceRate = Math.Round(orderAcceptanceRate, 2),
                    OrderFulfillmentRate = Math.Round(orderFulfillmentRate, 2),
                    PeakHourCapacity = peakHourCapacity,
                    CapacityUtilization = Math.Round(capacityUtilization, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating performance metrics for restaurant {restaurantId}");
                throw;
            }
        }

        public async Task<List<PopularMenuItemDto>> GetTopSellingItemsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, int topCount = 10)
        {
            try
            {
                var orders = await _orderRepository.GetListAsync(o => 
                    o.RestaurantId == restaurantId &&
                    o.OrderDate >= fromDate &&
                    o.OrderDate <= toDate &&
                    o.Status == OrderStatus.Delivered);

                var orderIds = orders.Select(o => o.Id).ToList();
                var orderItems = await _orderItemRepository.GetListAsync(oi => orderIds.Contains(oi.OrderId));

                var itemSummary = orderItems
                    .GroupBy(oi => oi.MenuItemId)
                    .Select(g => new
                    {
                        MenuItemId = g.Key,
                        TotalOrders = g.Select(x => x.OrderId).Distinct().Count(),
                        TotalQuantity = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.Price * x.Quantity),
                        ItemName = g.First().Name,
                        Price = g.First().Price
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(topCount)
                    .ToList();

                var result = new List<PopularMenuItemDto>();
                foreach (var item in itemSummary)
                {
                    var menuItem = await _menuItemRepository.GetAsync(item.MenuItemId);
                    var popularityScore = CalculatePopularityScore(item.TotalOrders, item.TotalRevenue, orders.Count);

                    result.Add(new PopularMenuItemDto
                    {
                        MenuItemId = item.MenuItemId,
                        ItemName = item.ItemName,
                        Category = menuItem.MealCategory?.Name ?? "Unknown",
                        TotalOrders = item.TotalOrders,
                        TotalQuantitySold = item.TotalQuantity,
                        TotalRevenue = item.TotalRevenue,
                        Price = item.Price,
                        PopularityScore = Math.Round(popularityScore, 2)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting top selling items for restaurant {restaurantId}");
                return new List<PopularMenuItemDto>();
            }
        }

        public async Task<List<DailyOrderSummaryDto>> GetDailyBreakdownAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var orders = await _orderRepository.GetListAsync(o => 
                    o.RestaurantId == restaurantId &&
                    o.OrderDate >= fromDate &&
                    o.OrderDate <= toDate);

                var dailyData = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailyOrderSummaryDto
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                        Commission = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount * 0.15m), // Default 15%
                        CompletedOrders = g.Count(o => o.Status == OrderStatus.Delivered),
                        CancelledOrders = g.Count(o => o.Status == OrderStatus.Cancelled),
                        AveragePreparationTime = 0 // Will be calculated separately if needed
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return dailyData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting daily breakdown for restaurant {restaurantId}");
                return new List<DailyOrderSummaryDto>();
            }
        }

        public async Task<List<HourlyOrderVolumeDto>> GetHourlyOrderVolumeAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var orders = await _orderRepository.GetListAsync(o => 
                    o.RestaurantId == restaurantId &&
                    o.OrderDate >= fromDate &&
                    o.OrderDate <= toDate);

                var hourlyData = orders
                    .GroupBy(o => o.OrderDate.Hour)
                    .Select(g => new HourlyOrderVolumeDto
                    {
                        Hour = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                        AveragePreparationTime = 0 // Will be calculated if preparation time tracking is implemented
                    })
                    .OrderBy(h => h.Hour)
                    .ToList();

                // Fill missing hours with zero data
                var completeHourlyData = new List<HourlyOrderVolumeDto>();
                for (int hour = 0; hour < 24; hour++)
                {
                    var data = hourlyData.FirstOrDefault(h => h.Hour == hour);
                    completeHourlyData.Add(data ?? new HourlyOrderVolumeDto
                    {
                        Hour = hour,
                        OrderCount = 0,
                        Revenue = 0,
                        AveragePreparationTime = 0
                    });
                }

                return completeHourlyData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting hourly order volume for restaurant {restaurantId}");
                return new List<HourlyOrderVolumeDto>();
            }
        }

        public async Task<CommissionSummaryDto> GetCommissionSummaryAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                var orders = await _orderRepository.GetListAsync(o => 
                    o.RestaurantId == restaurantId &&
                    o.OrderDate >= fromDate &&
                    o.OrderDate <= toDate &&
                    o.Status == OrderStatus.Delivered);

                var commissionRate = restaurant.CommissionRate ?? 0.15m;
                var totalOrderValue = orders.Sum(o => o.TotalAmount);
                var totalCommission = totalOrderValue * commissionRate;

                var dailyBreakdown = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailyCommissionDto
                    {
                        Date = g.Key,
                        OrderValue = g.Sum(o => o.TotalAmount),
                        CommissionDue = g.Sum(o => o.TotalAmount) * commissionRate,
                        OrderCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return new CommissionSummaryDto
                {
                    RestaurantId = restaurantId,
                    RestaurantName = restaurant.Name,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalOrderValue = totalOrderValue,
                    TotalCommissionDue = totalCommission,
                    CommissionRate = commissionRate,
                    TotalOrders = orders.Count,
                    DailyBreakdown = dailyBreakdown
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting commission summary for restaurant {restaurantId}");
                throw;
            }
        }

        #region Not Implemented Methods (Stubs for Future Enhancement)

        public async Task<YearlyRestaurantSummaryDto> GenerateYearlySummaryAsync(Guid restaurantId, int year)
        {
            // Placeholder implementation - would aggregate monthly data
            await Task.CompletedTask;
            throw new NotImplementedException("Yearly summary report not yet implemented");
        }

        public async Task<CustomReportDto> GenerateCustomReportAsync(DateRangeReportRequestDto request)
        {
            // Placeholder implementation - would generate custom reports based on type
            await Task.CompletedTask;
            throw new NotImplementedException("Custom reports not yet implemented");
        }

        public async Task<byte[]> ExportReportAsync(object reportData, string format)
        {
            // Placeholder implementation - would export to PDF/Excel/CSV
            await Task.CompletedTask;
            throw new NotImplementedException("Report export not yet implemented");
        }

        public async Task<bool> ScheduleAutomaticReportAsync(Guid restaurantId, string frequency, string email)
        {
            // Placeholder implementation - would schedule automatic report generation
            await Task.CompletedTask;
            throw new NotImplementedException("Automatic report scheduling not yet implemented");
        }

        #endregion

        #region Private Helper Methods

        private async Task<double> CalculateAveragePreparationTimeAsync(List<Order> completedOrders)
        {
            // Placeholder - would calculate based on order timestamps and status changes
            await Task.CompletedTask;
            return 25.0; // Default average preparation time in minutes
        }

        private async Task<double> CalculateAverageRatingAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var reviews = await _reviewRepository.GetListAsync(r => 
                    r.RestaurantId == restaurantId &&
                    r.Date >= fromDate &&
                    r.Date <= toDate);

                return reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        private async Task<int> CalculateReturningCustomersAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var orders = await _orderRepository.GetListAsync(o => 
                    o.RestaurantId == restaurantId &&
                    o.OrderDate >= fromDate &&
                    o.OrderDate <= toDate);

                var customerOrderCounts = orders
                    .GroupBy(o => o.UserId)
                    .Where(g => g.Count() > 1)
                    .Count();

                return customerOrderCounts;
            }
            catch
            {
                return 0;
            }
        }

        private OrderStatusDistributionDto CalculateStatusDistribution(List<Order> orders)
        {
            return new OrderStatusDistributionDto
            {
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                PreparingOrders = orders.Count(o => o.Status == OrderStatus.Preparing),
                ReadyForDeliveryOrders = orders.Count(o => o.Status == OrderStatus.ReadyForDelivery),
                DeliveringOrders = orders.Count(o => o.Status == OrderStatus.Delivering),
                DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled)
            };
        }

        private double CalculatePopularityScore(int totalOrders, decimal totalRevenue, int totalOrdersInPeriod)
        {
            // Simple popularity score calculation based on frequency and revenue contribution
            var frequencyScore = totalOrdersInPeriod > 0 ? (double)totalOrders / totalOrdersInPeriod * 100 : 0;
            var revenueScore = (double)totalRevenue; // Could be normalized against total revenue

            return (frequencyScore + revenueScore / 100) / 2; // Simple average
        }

        private async Task<double> CalculateAverageOrderToDeliveryTimeAsync(List<Order> completedOrders)
        {
            await Task.CompletedTask;
            return 45.0; // Placeholder
        }

        private async Task<double> CalculateOnTimeDeliveryRateAsync(List<Order> completedOrders)
        {
            await Task.CompletedTask;
            return 85.0; // Placeholder
        }

        private async Task<int> GetReviewCountAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var reviews = await _reviewRepository.GetListAsync(r => 
                    r.RestaurantId == restaurantId &&
                    r.Date >= fromDate &&
                    r.Date <= toDate);
                return reviews.Count();
            }
            catch
            {
                return 0;
            }
        }

        private async Task<double> CalculateCustomerSatisfactionScoreAsync(Guid restaurantId, DateTime fromDate, DateTime toDate)
        {
            var averageRating = await CalculateAverageRatingAsync(restaurantId, fromDate, toDate);
            return averageRating * 20; // Convert 5-star rating to 100-point scale
        }

        private double CalculateOrderAcceptanceRate(List<Order> orders)
        {
            var totalOrders = orders.Count;
            var acceptedOrders = orders.Count(o => o.Status != OrderStatus.Cancelled);
            return totalOrders > 0 ? (double)acceptedOrders / totalOrders * 100 : 100;
        }

        private double CalculateOrderFulfillmentRate(List<Order> orders)
        {
            var totalOrders = orders.Count;
            var fulfilledOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
            return totalOrders > 0 ? (double)fulfilledOrders / totalOrders * 100 : 0;
        }

        private async Task<int> CalculatePeakHourCapacityAsync(List<Order> orders)
        {
            await Task.CompletedTask;
            var hourlyOrderCounts = orders
                .GroupBy(o => o.OrderDate.Hour)
                .Select(g => g.Count())
                .DefaultIfEmpty(0);

            return hourlyOrderCounts.Max();
        }

        private async Task<double> CalculateCapacityUtilizationAsync(List<Order> orders, int peakCapacity)
        {
            await Task.CompletedTask;
            if (peakCapacity == 0) return 0;

            var averageHourlyOrders = orders.Count / 24.0; // Assuming 24-hour operation
            return averageHourlyOrders / peakCapacity * 100;
        }

        #endregion
    }
}
