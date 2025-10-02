using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.EntityFrameworkCore.Repositories
{
    public class OrderRepository : EfCoreRepository<DeliveryAppDbContext, Order, Guid>, IOrderRepository
    {
        public OrderRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId, int skipCount, int maxResultCount, string sorting)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.PaymentMethod)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<int> GetOrdersCountByUserIdAsync(Guid userId)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(o => o.UserId == userId)
                .CountAsync();
        }

        public async Task<List<Order>> GetOrdersByRestaurantIdAsync(Guid restaurantId, int skipCount, int maxResultCount, string sorting)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.User)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.PaymentMethod)
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.OrderDate)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<int> GetOrdersCountByRestaurantIdAsync(Guid restaurantId)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(o => o.RestaurantId == restaurantId)
                .CountAsync();
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status, int skipCount, int maxResultCount, string sorting)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.PaymentMethod)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<int> GetOrdersCountByStatusAsync(OrderStatus status)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Where(o => o.Status == status)
                .CountAsync();
        }

        public async Task<List<Order>> GetUserOrdersAsync(Guid userId, OrderStatus? status, int skipCount, int maxResultCount)
        {
            var query = await GetQueryableAsync();
            
            var ordersQuery = query
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.PaymentMethod)
                .Where(o => o.UserId == userId);

            if (status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status.Value);
            }

            return await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<List<Order>> GetRestaurantOrdersAsync(Guid restaurantId, OrderStatus? status, int skipCount, int maxResultCount)
        {
            var query = await GetQueryableAsync();
            
            var ordersQuery = query
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.User)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.PaymentMethod)
                .Where(o => o.RestaurantId == restaurantId);

            if (status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status.Value);
            }

            return await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<List<Order>> GetUserOrdersAsync(Guid userId, OrderStatus? status)
        {
            var query = await GetQueryableAsync();
            
            var ordersQuery = query
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.PaymentMethod)
                .Where(o => o.UserId == userId);

            if (status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status.Value);
            }

            return await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderWithDetailsAsync(Guid orderId)
        {
            var query = await GetQueryableAsync();
            
            return await query
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.PaymentMethod)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
