using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IChatAppService : IApplicationService
    {
        Task<ChatSessionDto> StartChatSessionAsync(StartDeliveryChatSessionDto input);
        Task<ChatMessageDto> SendMessageAsync(SendDeliveryChatMessageDto input);
        Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId);
        Task MarkMessagesAsReadAsync(MarkMessagesAsReadDto input);
        Task<ChatSessionDto> GetChatSessionByDeliveryIdAsync(Guid deliveryId);
        Task<ChatSessionWithMessagesDto> GetChatSessionWithMessagesAsync(Guid sessionId);
        Task<List<ChatSessionDto>> GetActiveChatSessionsAsync();
        Task<List<ChatMessageDto>> GetNewMessagesAsync(Guid sessionId, DateTime since);
        Task CloseChatSessionAsync(Guid sessionId);
        Task<List<ChatSessionDto>> GetChatHistoryAsync(Guid deliveryId);
    }
}
