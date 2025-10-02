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
    public class ChatSessionRepository : EfCoreRepository<DeliveryAppDbContext, ChatSession, Guid>, IChatSessionRepository
    {
        public ChatSessionRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ChatSession> GetByDeliveryIdAsync(Guid deliveryId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.DeliveryId == deliveryId && x.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ChatSession>> GetActiveSessionsAsync()
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatSession> GetWithMessagesAsync(Guid sessionId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.Id == sessionId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ChatSession>> GetByDeliveryIdHistoryAsync(Guid deliveryId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.DeliveryId == deliveryId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
