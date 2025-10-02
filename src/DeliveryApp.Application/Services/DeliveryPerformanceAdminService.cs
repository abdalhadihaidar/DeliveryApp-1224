using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Service for delivery performance analytics and reporting for admin users
    /// </summary>
    public class DeliveryPerformanceAdminService : ApplicationService, IDeliveryPerformanceAdminService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Review, Guid> _reviewRepository;
        private readonly ILogger<DeliveryPerformanceAdminService> _logger;

        public DeliveryPerformanceAdminService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IRepository<Review, Guid> reviewRepository,
            ILogger<DeliveryPerformanceAdminService> logger)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<DeliveryPersonPerformanceDto[]> GetDeliveryPerformanceAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Get all delivery persons using efficient role-based query
                var deliveryUsers = await _userRepository.GetUsersByRoleAsync("delivery");

                _logger.LogInformation($"Found {deliveryUsers.Count} delivery persons in the system");
                
                var performanceData = new List<DeliveryPersonPerformanceDto>();

                foreach (var deliveryPerson in deliveryUsers)
                {
                    var performance = await CalculateDeliveryPersonPerformanceAsync(
                        deliveryPerson.Id, 
                        deliveryPerson.Name ?? deliveryPerson.Email, 
                        fromDate, 
                        toDate);
                    
                    if (performance != null)
                    {
                        performanceData.Add(performance);
                    }
                }

                return performanceData.OrderByDescending(p => p.Rating).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery performance data for date range {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<DetailedDeliveryPerformanceDto> GetDetailedPerformanceAsync(Guid deliveryPersonId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                
                // Get all orders for this delivery person in the date range
                var orders = await _orderRepository.GetQueryableAsync();
                var deliveryOrders = await orders
                    .Where(o => o.DeliveryPersonId == deliveryPersonId && 
                               o.OrderDate >= fromDate && 
                               o.OrderDate <= toDate)
                    .Include(o => o.Items)
                    .ToListAsync();

                // Get reviews for this delivery person
                var reviews = await _reviewRepository.GetQueryableAsync();
                var deliveryReviews = await reviews
                    .Where(r => r.UserId == deliveryPersonId && 
                               r.Date >= fromDate && 
                               r.Date <= toDate)
                    .ToListAsync();

                var completedOrders = deliveryOrders.Where(o => o.Status == OrderStatus.Delivered).ToList();
                var cancelledOrders = deliveryOrders.Where(o => o.Status == OrderStatus.Cancelled).ToList();

                // Calculate performance metrics
                var totalEarnings = completedOrders.Sum(o => o.DeliveryFee);
                var averageDeliveryTime = completedOrders.Any() 
                    ? completedOrders.Average(o => CalculateDeliveryTime(o)) 
                    : 0;
                var averageRating = deliveryReviews.Any() 
                    ? deliveryReviews.Average(r => r.Rating) 
                    : 0;

                var onTimeDeliveries = completedOrders.Count(o => IsDeliveryOnTime(o));
                var lateDeliveries = completedOrders.Count - onTimeDeliveries;
                var onTimePercentage = completedOrders.Any() 
                    ? (double)onTimeDeliveries / completedOrders.Count * 100 
                    : 0;

                // Calculate daily performance
                var dailyPerformance = completedOrders
                    .GroupBy(o => o.OrderDate.Date) // Use OrderDate since DeliveryDate doesn't exist
                    .Select(g => new DailyPerformanceDto
                    {
                        Date = g.Key,
                        DeliveryCount = g.Count(),
                        AverageDeliveryTime = g.Average(o => CalculateDeliveryTime(o)),
                        AverageRating = deliveryReviews.Where(r => r.Date.Date == g.Key).Any() 
                            ? deliveryReviews.Where(r => r.Date.Date == g.Key).Average(r => r.Rating) 
                            : 0,
                        TotalEarnings = g.Sum(o => o.DeliveryFee),
                        OnTimeDeliveries = g.Count(o => IsDeliveryOnTime(o)),
                        LateDeliveries = g.Count(o => !IsDeliveryOnTime(o))
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return new DetailedDeliveryPerformanceDto
                {
                    DeliveryPersonId = deliveryPersonId.ToString(),
                    Name = deliveryPerson.Name ?? deliveryPerson.Email,
                    Email = deliveryPerson.Email,
                    Phone = deliveryPerson.PhoneNumber ?? string.Empty,
                    FromDate = fromDate,
                    ToDate = toDate,
                    
                    // Order Statistics
                    TotalOrders = deliveryOrders.Count,
                    CompletedOrders = completedOrders.Count,
                    CancelledOrders = cancelledOrders.Count,
                    CompletionRate = deliveryOrders.Any() 
                        ? (double)completedOrders.Count / deliveryOrders.Count * 100 
                        : 0,
                    
                    // Time Performance
                    AverageDeliveryMinutes = Math.Round(averageDeliveryTime, 2),
                    FastestDeliveryMinutes = completedOrders.Any() 
                        ? Math.Round(completedOrders.Min(o => CalculateDeliveryTime(o)), 2) 
                        : 0,
                    SlowestDeliveryMinutes = completedOrders.Any() 
                        ? Math.Round(completedOrders.Max(o => CalculateDeliveryTime(o)), 2) 
                        : 0,
                    OnTimeDeliveries = onTimeDeliveries,
                    LateDeliveries = lateDeliveries,
                    OnTimePercentage = Math.Round(onTimePercentage, 2),
                    
                    // Quality Metrics
                    AverageRating = Math.Round(averageRating, 2),
                    TotalRatings = deliveryReviews.Count(),
                    FiveStarRatings = deliveryReviews.Count(r => r.Rating >= 5),
                    FourStarRatings = deliveryReviews.Count(r => r.Rating >= 4 && r.Rating < 5),
                    ThreeStarRatings = deliveryReviews.Count(r => r.Rating >= 3 && r.Rating < 4),
                    TwoStarRatings = deliveryReviews.Count(r => r.Rating >= 2 && r.Rating < 3),
                    OneStarRatings = deliveryReviews.Count(r => r.Rating >= 1 && r.Rating < 2),
                    
                    // Financial Metrics
                    TotalEarnings = totalEarnings,
                    AverageEarningsPerDelivery = completedOrders.Any() 
                        ? Math.Round(totalEarnings / completedOrders.Count, 2) 
                        : 0,
                    TotalTips = 0, // TipAmount property doesn't exist in Order entity
                    
                    // Daily Performance
                    DailyPerformance = dailyPerformance,
                    ZonePerformance = new List<DeliveryZonePerformanceDto>() // Empty for now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed performance for delivery person {DeliveryPersonId}", deliveryPersonId);
                throw;
            }
        }

        public async Task<DeliveryPerformanceSummaryDto> GetPerformanceSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Get all delivery persons using efficient role-based query
                var deliveryUsers = await _userRepository.GetUsersByRoleAsync("delivery");

                // Get all orders in date range
                var orders = await _orderRepository.GetQueryableAsync();
                var allOrders = await orders
                    .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                    .ToListAsync();

                // Get all reviews in date range
                var reviews = await _reviewRepository.GetQueryableAsync();
                var allReviews = await reviews
                    .Where(r => r.Date >= fromDate && r.Date <= toDate)
                    .ToListAsync();

                var completedOrders = allOrders.Where(o => o.Status == OrderStatus.Delivered).ToList();

                // Calculate summary statistics
                var averageDeliveryTime = completedOrders.Any() 
                    ? completedOrders.Average(o => CalculateDeliveryTime(o)) 
                    : 0;
                var averageRating = allReviews.Any() 
                    ? allReviews.Average(r => r.Rating) 
                    : 0;

                var onTimeDeliveries = completedOrders.Count(o => IsDeliveryOnTime(o));
                var lateDeliveries = completedOrders.Count - onTimeDeliveries;
                var onTimePercentage = completedOrders.Any() 
                    ? (double)onTimeDeliveries / completedOrders.Count * 100 
                    : 0;

                // Performance distribution
                var excellentPerformers = 0;
                var goodPerformers = 0;
                var averagePerformers = 0;
                var poorPerformers = 0;

                foreach (var deliveryPerson in deliveryUsers)
                {
                    var personReviews = allReviews.Where(r => r.UserId == deliveryPerson.Id);
                    if (personReviews.Any())
                    {
                        var personRating = personReviews.Average(r => r.Rating);
                        if (personRating >= 4.5) excellentPerformers++;
                        else if (personRating >= 3.5) goodPerformers++;
                        else if (personRating >= 2.5) averagePerformers++;
                        else poorPerformers++;
                    }
                }

                return new DeliveryPerformanceSummaryDto
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalDeliveryPersons = deliveryUsers.Count(),
                    ActiveDeliveryPersons = deliveryUsers.Count(u => u.IsActive),
                    TotalDeliveries = completedOrders.Count,
                    AverageDeliveryTime = Math.Round(averageDeliveryTime, 2),
                    AverageRating = Math.Round(averageRating, 2),
                    ExcellentPerformers = excellentPerformers,
                    GoodPerformers = goodPerformers,
                    AveragePerformers = averagePerformers,
                    PoorPerformers = poorPerformers,
                    AverageOnTimePercentage = Math.Round(onTimePercentage, 2),
                    TotalOnTimeDeliveries = onTimeDeliveries,
                    TotalLateDeliveries = lateDeliveries,
                    TopPerformers = new List<TopPerformerDto>() // Empty for now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance summary for date range {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        private async Task<DeliveryPersonPerformanceDto?> CalculateDeliveryPersonPerformanceAsync(
            Guid deliveryPersonId, 
            string name, 
            DateTime fromDate, 
            DateTime toDate)
        {
            try
            {
                // Get orders for this delivery person
                var orders = await _orderRepository.GetQueryableAsync();
                var deliveryOrders = await orders
                    .Where(o => o.DeliveryPersonId == deliveryPersonId && 
                               o.OrderDate >= fromDate && 
                               o.OrderDate <= toDate)
                    .ToListAsync();

                if (!deliveryOrders.Any())
                {
                    return null; // No orders in this period
                }

                var completedOrders = deliveryOrders.Where(o => o.Status == OrderStatus.Delivered).ToList();

                // Get reviews for this delivery person
                var reviews = await _reviewRepository.GetQueryableAsync();
                var deliveryReviews = await reviews
                    .Where(r => r.UserId == deliveryPersonId && 
                               r.Date >= fromDate && 
                               r.Date <= toDate)
                    .ToListAsync();

                var averageDeliveryTime = completedOrders.Any() 
                    ? completedOrders.Average(o => CalculateDeliveryTime(o)) 
                    : 0;
                var averageRating = deliveryReviews.Any() 
                    ? deliveryReviews.Average(r => r.Rating) 
                    : 0;
                var totalEarnings = completedOrders.Sum(o => o.DeliveryFee);
                var onTimeDeliveries = completedOrders.Count(o => IsDeliveryOnTime(o));
                var lateDeliveries = completedOrders.Count - onTimeDeliveries;
                var onTimePercentage = completedOrders.Any() 
                    ? (double)onTimeDeliveries / completedOrders.Count * 100 
                    : 0;

                var lastDeliveryDate = completedOrders.Any() 
                    ? completedOrders.Max(o => o.OrderDate) 
                    : DateTime.MinValue;

                return new DeliveryPersonPerformanceDto
                {
                    DeliveryPersonId = deliveryPersonId.ToString(),
                    Name = name,
                    CompletedOrders = completedOrders.Count,
                    AverageDeliveryMinutes = Math.Round(averageDeliveryTime, 2),
                    Rating = Math.Round(averageRating, 2),
                    TotalEarnings = totalEarnings,
                    OnTimeDeliveries = onTimeDeliveries,
                    LateDeliveries = lateDeliveries,
                    OnTimePercentage = Math.Round(onTimePercentage, 2),
                    LastDeliveryDate = lastDeliveryDate,
                    Status = "Active" // You can determine this based on your business logic
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating performance for delivery person {DeliveryPersonId}", deliveryPersonId);
                return null;
            }
        }

        private double CalculateDeliveryTime(Order order)
        {
            // Use EstimatedDeliveryTime from the order, or default to 45 minutes
            // Since DeliveryDate doesn't exist, we'll use a calculated approach
            var estimatedDeliveryTime = order.EstimatedDeliveryTime; // in minutes
            return estimatedDeliveryTime > 0 ? estimatedDeliveryTime : 45; // Default 45 minutes
        }

        private bool IsDeliveryOnTime(Order order)
        {
            // Consider delivery on time if it's within the estimated delivery time
            // You can adjust this logic based on your business rules
            var deliveryTime = CalculateDeliveryTime(order);
            return deliveryTime <= 60; // Assuming 60 minutes is the standard delivery time
        }
    }
}
