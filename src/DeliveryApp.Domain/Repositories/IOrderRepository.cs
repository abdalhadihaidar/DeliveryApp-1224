using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId, int skipCount, int maxResultCount, string sorting);
        
        Task<int> GetOrdersCountByUserIdAsync(Guid userId);
        
        Task<List<Order>> GetOrdersByRestaurantIdAsync(Guid restaurantId, int skipCount, int maxResultCount, string sorting);
        
        Task<int> GetOrdersCountByRestaurantIdAsync(Guid restaurantId);
        
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status, int skipCount, int maxResultCount, string sorting);
        
        Task<int> GetOrdersCountByStatusAsync(OrderStatus status);
        
        Task<List<Order>> GetUserOrdersAsync(Guid userId, OrderStatus? status, int skipCount, int maxResultCount);
        
        Task<List<Order>> GetRestaurantOrdersAsync(Guid restaurantId, OrderStatus? status, int skipCount, int maxResultCount);
        Task<List<Order>> GetUserOrdersAsync(Guid userId, OrderStatus? status);
        Task<Order> GetOrderWithDetailsAsync(Guid orderId);
    }
}
