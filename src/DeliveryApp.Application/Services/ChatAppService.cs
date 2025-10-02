using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp;
using ChatSessionEntity = DeliveryApp.Domain.Entities.ChatSession;
using ChatMessageEntity = DeliveryApp.Domain.Entities.ChatMessage;

namespace DeliveryApp.Application.Services
{
    public class ChatAppService : ApplicationService, IChatAppService
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ChatAppService> _logger;

        public ChatAppService(
            IChatSessionRepository chatSessionRepository,
            IChatMessageRepository chatMessageRepository,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            ILogger<ChatAppService> logger)
        {
            _chatSessionRepository = chatSessionRepository;
            _chatMessageRepository = chatMessageRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ChatSessionDto> StartChatSessionAsync(StartDeliveryChatSessionDto input)
        {
            try
            {
                // Check if session already exists for this delivery
                var existingSession = await _chatSessionRepository.GetByDeliveryIdAsync(input.DeliveryId);
                if (existingSession != null && existingSession.IsActive)
                {
                    _logger.LogInformation($"Active chat session already exists for delivery {input.DeliveryId}");
                    return ObjectMapper.Map<ChatSessionEntity, ChatSessionDto>(existingSession);
                }

                // Verify delivery exists
                var delivery = await _orderRepository.GetAsync(input.DeliveryId);
                if (delivery == null)
                {
                    throw new UserFriendlyException($"Delivery with ID {input.DeliveryId} not found");
                }

                // Look up customer by phone number if CustomerId is a phone number
                Guid customerId;
                if (Guid.TryParse(input.CustomerId, out var customerGuid))
                {
                    customerId = customerGuid;
                }
                else
                {
                    // CustomerId is a phone number, look up the customer
                    var customer = await _userRepository.GetByPhoneNumberAsync(input.CustomerId);
                    if (customer == null)
                    {
                        throw new UserFriendlyException($"Customer with phone number {input.CustomerId} not found");
                    }
                    customerId = customer.Id;
                }

                // Convert AdminId from string to Guid
                if (!Guid.TryParse(input.AdminId, out var adminId))
                {
                    throw new UserFriendlyException($"Invalid AdminId format: {input.AdminId}");
                }

                // Create new chat session
                var session = new ChatSessionEntity(
                    GuidGenerator.Create(),
                    input.DeliveryId,
                    customerId,
                    input.CustomerPhoneNumber,
                    adminId,
                    input.AdminName
                );

                var createdSession = await _chatSessionRepository.InsertAsync(session);

                // Add system message
                var systemMessage = new ChatMessageEntity(
                    GuidGenerator.Create(),
                    createdSession.Id,
                    adminId,
                    "system",
                    $"Chat session started by {input.AdminName}",
                    "system"
                );

                await _chatMessageRepository.InsertAsync(systemMessage);

                _logger.LogInformation($"Chat session created for delivery {input.DeliveryId}");

                return ObjectMapper.Map<ChatSessionEntity, ChatSessionDto>(createdSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to start chat session for delivery {input.DeliveryId}");
                throw;
            }
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendDeliveryChatMessageDto input)
        {
            try
            {
                // Verify session exists
                var session = await _chatSessionRepository.GetAsync(input.SessionId);
                if (session == null)
                {
                    throw new UserFriendlyException($"Chat session with ID {input.SessionId} not found");
                }

                // Convert SenderId from string to Guid
                Guid senderId;
                if (input.SenderId == "admin" || input.SenderType == "admin")
                {
                    // For admin messages, use the admin ID from the session
                    senderId = session.AdminId;
                }
                else if (!Guid.TryParse(input.SenderId, out senderId))
                {
                    throw new UserFriendlyException($"Invalid SenderId format: {input.SenderId}");
                }

                // Create message
                var message = new ChatMessageEntity(
                    GuidGenerator.Create(),
                    input.SessionId,
                    senderId,
                    input.SenderType,
                    input.Content,
                    input.MessageType
                );

                var createdMessage = await _chatMessageRepository.InsertAsync(message);

                // Update session last message time
                session.LastMessageAt = DateTime.UtcNow;
                await _chatSessionRepository.UpdateAsync(session);

                _logger.LogInformation($"Message sent in chat session {input.SessionId}");

                return ObjectMapper.Map<ChatMessageEntity, ChatMessageDto>(createdMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send message in session {input.SessionId}");
                throw;
            }
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId)
        {
            try
            {
                var messages = await _chatMessageRepository.GetBySessionIdAsync(sessionId);
                return ObjectMapper.Map<List<ChatMessageEntity>, List<ChatMessageDto>>(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get messages for session {sessionId}");
                throw;
            }
        }

        public async Task MarkMessagesAsReadAsync(MarkMessagesAsReadDto input)
        {
            try
            {
                await _chatMessageRepository.MarkMessagesAsReadAsync(input.SessionId, input.UserId);
                _logger.LogInformation($"Messages marked as read for session {input.SessionId} by user {input.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark messages as read for session {input.SessionId}");
                throw;
            }
        }

        public async Task<ChatSessionDto> GetChatSessionByDeliveryIdAsync(Guid deliveryId)
        {
            try
            {
                var session = await _chatSessionRepository.GetByDeliveryIdAsync(deliveryId);
                if (session == null)
                {
                    return null;
                }

                return ObjectMapper.Map<ChatSessionEntity, ChatSessionDto>(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get chat session for delivery {deliveryId}");
                throw;
            }
        }

        public async Task<ChatSessionWithMessagesDto> GetChatSessionWithMessagesAsync(Guid sessionId)
        {
            try
            {
                var session = await _chatSessionRepository.GetWithMessagesAsync(sessionId);
                if (session == null)
                {
                    throw new UserFriendlyException($"Chat session with ID {sessionId} not found");
                }

                var sessionDto = ObjectMapper.Map<ChatSessionEntity, ChatSessionDto>(session);
                var messages = await _chatMessageRepository.GetBySessionIdAsync(sessionId);
                var messageDtos = ObjectMapper.Map<List<ChatMessageEntity>, List<ChatMessageDto>>(messages);

                return new ChatSessionWithMessagesDto
                {
                    Id = sessionDto.Id,
                    DeliveryId = sessionDto.DeliveryId,
                    CustomerId = sessionDto.CustomerId,
                    CustomerPhoneNumber = sessionDto.CustomerPhoneNumber,
                    AdminId = sessionDto.AdminId,
                    AdminName = sessionDto.AdminName,
                    CreatedAt = sessionDto.CreatedAt,
                    LastMessageAt = sessionDto.LastMessageAt,
                    IsActive = sessionDto.IsActive,
                    Messages = messageDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get chat session with messages for session {sessionId}");
                throw;
            }
        }

        public async Task<List<ChatSessionDto>> GetActiveChatSessionsAsync()
        {
            try
            {
                var sessions = await _chatSessionRepository.GetActiveSessionsAsync();
                return ObjectMapper.Map<List<ChatSessionEntity>, List<ChatSessionDto>>(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active chat sessions");
                throw;
            }
        }

        public async Task<List<ChatMessageDto>> GetNewMessagesAsync(Guid sessionId, DateTime since)
        {
            try
            {
                var messages = await _chatMessageRepository.GetNewMessagesAsync(sessionId, since);
                return ObjectMapper.Map<List<ChatMessageEntity>, List<ChatMessageDto>>(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get new messages for session {sessionId}");
                throw;
            }
        }

        public async Task CloseChatSessionAsync(Guid sessionId)
        {
            try
            {
                var session = await _chatSessionRepository.GetAsync(sessionId);
                if (session != null)
                {
                    session.IsActive = false;
                    await _chatSessionRepository.UpdateAsync(session);
                    _logger.LogInformation($"Chat session {sessionId} closed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to close chat session {sessionId}");
                throw;
            }
        }

        public async Task<List<ChatSessionDto>> GetChatHistoryAsync(Guid deliveryId)
        {
            try
            {
                var sessions = await _chatSessionRepository.GetByDeliveryIdHistoryAsync(deliveryId);
                return ObjectMapper.Map<List<ChatSessionEntity>, List<ChatSessionDto>>(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get chat history for delivery {deliveryId}");
                throw;
            }
        }
    }
}
