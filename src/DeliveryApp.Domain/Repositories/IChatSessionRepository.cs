using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IChatSessionRepository : IRepository<ChatSession, Guid>
    {
        Task<ChatSession> GetByDeliveryIdAsync(Guid deliveryId);
        Task<List<ChatSession>> GetActiveSessionsAsync();
        Task<ChatSession> GetWithMessagesAsync(Guid sessionId);
        Task<List<ChatSession>> GetByDeliveryIdHistoryAsync(Guid deliveryId);
    }
}
