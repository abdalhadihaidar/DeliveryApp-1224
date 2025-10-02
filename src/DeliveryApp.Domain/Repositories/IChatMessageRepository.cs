using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IChatMessageRepository : IRepository<ChatMessage, Guid>
    {
        Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId);
        Task<List<ChatMessage>> GetUnreadMessagesAsync(Guid sessionId, Guid userId);
        Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId);
        Task<List<ChatMessage>> GetNewMessagesAsync(Guid sessionId, DateTime since);
    }
}
