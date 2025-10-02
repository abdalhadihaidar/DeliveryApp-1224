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
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Application.Services
{
    public class DeliveryPerformanceService : ApplicationService, IDeliveryPerformanceService, ITransientDependency
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public DeliveryPerformanceService(IOrderRepository orderRepository, IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public async Task<List<DeliveryPersonPerformanceDto>> GetPerformanceAsync(DateTime fromDate, DateTime toDate)
        {
            // Optimize query to avoid N+1 problem by grouping in database
            var queryable = await _orderRepository.GetQueryableAsync();
            var performanceData = await queryable
                .Where(o => o.Status == OrderStatus.Delivered && 
                           o.DeliveryPersonId.HasValue &&
                           o.CreationTime >= fromDate && 
                           o.CreationTime <= toDate)
                .GroupBy(o => o.DeliveryPersonId!.Value)
                .Select(g => new
                {
                    DeliveryPersonId = g.Key,
                    Orders = g.ToList(),
                    CompletedOrders = g.Count(),
                    AverageDeliveryMinutes = g.Average(o => 
                        (o.LastModificationTime ?? o.CreationTime).Subtract(o.CreationTime).TotalMinutes),
                    TotalEarnings = g.Sum(o => o.DeliveryFee),
                    OnTimeDeliveries = g.Count(o => IsDeliveryOnTime(o)),
                    LastDeliveryDate = g.Max(o => o.LastModificationTime ?? o.CreationTime)
                })
                .ToListAsync();

            // Get delivery person details in single query
            var deliveryPersonIds = performanceData.Select(p => p.DeliveryPersonId).ToList();
            var deliveryPersons = await _userRepository.GetListAsync(u => deliveryPersonIds.Contains(u.Id));
            var deliveryPersonLookup = deliveryPersons.ToDictionary(dp => dp.Id, dp => dp);

            var performanceList = performanceData.Select(p => new DeliveryPersonPerformanceDto
            {
                DeliveryPersonId = p.DeliveryPersonId.ToString(),
                Name = deliveryPersonLookup.TryGetValue(p.DeliveryPersonId, out var person) ? person.Name ?? "Unknown" : "Unknown",
                CompletedOrders = p.CompletedOrders,
                AverageDeliveryMinutes = Math.Round(p.AverageDeliveryMinutes, 2),
                Rating = 4.5, // Placeholder - would come from actual rating system
                TotalEarnings = p.TotalEarnings,
                OnTimeDeliveries = p.OnTimeDeliveries,
                LateDeliveries = p.CompletedOrders - p.OnTimeDeliveries,
                OnTimePercentage = p.CompletedOrders > 0 ? 
                    Math.Round((double)p.OnTimeDeliveries / p.CompletedOrders * 100, 2) : 0,
                LastDeliveryDate = p.LastDeliveryDate,
                Status = "Active"
            }).ToList();

            return performanceList.OrderByDescending(p => p.CompletedOrders).ToList();
        }

        private bool IsDeliveryOnTime(Order order)
        {
            // Consider delivery on time if it's within the estimated delivery time
            // You can adjust this logic based on your business rules
            var deliveryTime = (order.LastModificationTime ?? order.CreationTime).Subtract(order.CreationTime).TotalMinutes;
            return deliveryTime <= 60; // Assuming 60 minutes is the standard delivery time
        }
    }
}
