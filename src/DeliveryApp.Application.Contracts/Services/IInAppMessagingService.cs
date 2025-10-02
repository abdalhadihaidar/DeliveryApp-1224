using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Contracts.Services
{
    [RemoteService]
    public interface IInAppMessagingService : IApplicationService
    {
        Task<Conversation> CreateConversationAsync(CreateConversationDto dto);
        Task<Message> SendMessageAsync(SendMessageDto dto);
        Task<bool> MarkMessageAsReadAsync(string messageId, string userId);
        Task<List<Message>> GetConversationMessagesAsync(string conversationId, string userId, int page = 1, int pageSize = 50);
        Task<List<Conversation>> GetUserConversationsAsync(string userId, ConversationFilter filter = null);
        Task<bool> AddParticipantAsync(string conversationId, string userId, string addedBy);
        Task<bool> RemoveParticipantAsync(string conversationId, string userId, string removedBy);
        Task<bool> UpdateTypingStatusAsync(string conversationId, string userId, bool isTyping);
        Task<List<Message>> SearchMessagesAsync(string userId, MessageSearchCriteria criteria);
        Task<bool> DeleteMessageAsync(string messageId, string userId);
        Task<Message> EditMessageAsync(string messageId, string userId, string newContent);
        Task<MessagingAnalytics> GetMessagingAnalyticsAsync(string userId, MessagingAnalyticsFilter filter);
    }
}

