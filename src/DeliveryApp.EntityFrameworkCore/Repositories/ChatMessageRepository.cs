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
    public class ChatMessageRepository : EfCoreRepository<DeliveryAppDbContext, ChatMessage, Guid>, IChatMessageRepository
    {
        public ChatMessageRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.SessionId == sessionId)
                .OrderBy(x => x.SentAt)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetUnreadMessagesAsync(Guid sessionId, Guid userId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.SessionId == sessionId && !x.IsRead && x.SenderId != userId)
                .OrderBy(x => x.SentAt)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
        {
            var dbSet = await GetDbSetAsync();
            var unreadMessages = await dbSet
                .Where(x => x.SessionId == sessionId && !x.IsRead && x.SenderId != userId)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            if (unreadMessages.Any())
            {
                await UpdateManyAsync(unreadMessages);
            }
        }

        public async Task<List<ChatMessage>> GetNewMessagesAsync(Guid sessionId, DateTime since)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.SessionId == sessionId && x.SentAt > since)
                .OrderBy(x => x.SentAt)
                .ToListAsync();
        }
    }
}
