using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DeliveryApp.EntityFrameworkCore.Repositories
{
    public class CODTransactionRepository : EfCoreRepository<DeliveryAppDbContext, CODTransaction, Guid>, ICODTransactionRepository
    {
        public CODTransactionRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<CODTransaction>> GetTransactionsByDeliveryPersonAsync(Guid deliveryPersonId, int skipCount, int maxResultCount, string sorting)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(t => t.DeliveryPersonId == deliveryPersonId);

            if (!string.IsNullOrEmpty(sorting))
            {
                query = sorting.ToLower() switch
                {
                    "createdat desc" => query.OrderByDescending(t => t.CreationTime),
                    "createdat asc" => query.OrderBy(t => t.CreationTime),
                    "amount desc" => query.OrderByDescending(t => t.Amount),
                    "amount asc" => query.OrderBy(t => t.Amount),
                    "status desc" => query.OrderByDescending(t => t.Status),
                    "status asc" => query.OrderBy(t => t.Status),
                    _ => query.OrderByDescending(t => t.CreationTime)
                };
            }
            else
            {
                query = query.OrderByDescending(t => t.CreationTime);
            }

            return await query
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<int> GetTransactionsCountByDeliveryPersonAsync(Guid deliveryPersonId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.CountAsync(t => t.DeliveryPersonId == deliveryPersonId);
        }

        public async Task<List<CODTransaction>> GetTransactionsByOrderAsync(Guid orderId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(t => t.OrderId == orderId)
                .OrderByDescending(t => t.CreationTime)
                .ToListAsync();
        }

        public async Task<List<CODTransaction>> GetPendingTransactionsAsync()
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(t => t.Status == CODTransactionStatus.Pending)
                .OrderBy(t => t.CreationTime)
                .ToListAsync();
        }

        public async Task<CODTransaction?> GetActiveTransactionForOrderAsync(Guid orderId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .FirstOrDefaultAsync(t => t.OrderId == orderId && t.Status == CODTransactionStatus.Pending);
        }

        public async Task<List<CODTransaction>> GetTransactionsByStatusAsync(CODTransactionStatus status, int skipCount, int maxResultCount, string sorting)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(sorting))
            {
                query = sorting.ToLower() switch
                {
                    "createdat desc" => query.OrderByDescending(t => t.CreationTime),
                    "createdat asc" => query.OrderBy(t => t.CreationTime),
                    "amount desc" => query.OrderByDescending(t => t.Amount),
                    "amount asc" => query.OrderBy(t => t.Amount),
                    _ => query.OrderByDescending(t => t.CreationTime)
                };
            }
            else
            {
                query = query.OrderByDescending(t => t.CreationTime);
            }

            return await query
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public async Task<int> GetTransactionsCountByStatusAsync(CODTransactionStatus status)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.CountAsync(t => t.Status == status);
        }
    }
}
