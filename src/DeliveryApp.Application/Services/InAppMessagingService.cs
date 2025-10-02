using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Services;

namespace DeliveryApp.Application.Services
{
    // Local repository abstraction for messaging module (simple async CRUD)
    public interface IMessagingRepository<T>
    {
        Task<T> GetAsync(string id);
        Task<List<T>> GetListAsync(
           Func<T, bool> predicate = null,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           int? skip = null,
           int? take = null);
        Task<T> InsertAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(string id);
    }

    public class InAppMessagingService : IInAppMessagingService
    {
        private readonly IHubContext<MessagingHub> _hubContext;
        private readonly ILogger<InAppMessagingService> _logger;
        private readonly IMessagingRepository<Message> _messageRepository;
        private readonly IMessagingRepository<Conversation> _conversationRepository;
        private readonly IMessagingRepository<User> _userRepository;
        private readonly INotificationService _notificationService;

        public InAppMessagingService(
            IHubContext<MessagingHub> hubContext,
            ILogger<InAppMessagingService> logger,
            IMessagingRepository<Message> messageRepository,
            IMessagingRepository<Conversation> conversationRepository,
            IMessagingRepository<User> userRepository,
            INotificationService notificationService)
        {
            _hubContext = hubContext;
            _logger = logger;
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<Conversation> CreateConversationAsync(CreateConversationDto dto)
        {
            try
            {
                var conversation = new Conversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = dto.Type,
                    Title = dto.Title,
                    Description = dto.Description,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Participants = dto.ParticipantIds.Select(id => new ConversationParticipant
                    {
                        UserId = id,
                        JoinedAt = DateTime.UtcNow,
                        Role = id == dto.CreatedBy ? ParticipantRole.Admin : ParticipantRole.Member,
                        IsActive = true
                    }).ToList(),
                    Settings = new ConversationSettings
                    {
                        AllowFileSharing = true,
                        AllowMediaSharing = true,
                        MessageRetentionDays = 365,
                        IsEncrypted = true
                    }
                };

                await _conversationRepository.InsertAsync(conversation);

                // Notify participants
                foreach (var participantId in dto.ParticipantIds)
                {
                    await _hubContext.Clients.User(participantId)
                        .SendAsync("ConversationCreated", conversation);
                }

                _logger.LogInformation($"Conversation {conversation.Id} created by {dto.CreatedBy}");
                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating conversation: {ex.Message}");
                throw;
            }
        }

        public async Task<Message> SendMessageAsync(SendMessageDto dto)
        {
            try
            {
                var conversation = await _conversationRepository.GetAsync(dto.ConversationId);
                if (conversation == null)
                    throw new ArgumentException("Conversation not found");

                var participant = conversation.Participants.FirstOrDefault(p => p.UserId == dto.SenderId);
                if (participant == null || !participant.IsActive)
                    throw new UnauthorizedAccessException("User is not a participant in this conversation");

                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = dto.ConversationId,
                    SenderId = dto.SenderId,
                    Content = dto.Content,
                    Type = dto.Type,
                    SentAt = DateTime.UtcNow,
                    IsDelivered = false,
                    IsRead = false,
                    IsEdited = false,
                    Attachments = dto.Attachments?.Select(a => new MessageAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        FileName = a.FileName,
                        FileUrl = a.FileUrl,
                        FileType = a.FileType,
                        FileSize = a.FileSize,
                        ThumbnailUrl = a.ThumbnailUrl
                    }).ToList() ?? new List<MessageAttachment>(),
                    Metadata = dto.Metadata ?? new Dictionary<string, object>()
                };

                // Encrypt message if conversation requires it
                if (conversation.Settings.IsEncrypted)
                {
                    message.Content = await EncryptMessageAsync(message.Content);
                    message.IsEncrypted = true;
                }

                await _messageRepository.InsertAsync(message);

                // Update conversation last activity
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.LastMessageId = message.Id;
                conversation.UpdatedAt = DateTime.UtcNow;
                await _conversationRepository.UpdateAsync(conversation);

                // Send real-time notification to participants
                var activeParticipants = conversation.Participants
                    .Where(p => p.IsActive && p.UserId != dto.SenderId)
                    .Select(p => p.UserId)
                    .ToList();

                foreach (var participantId in activeParticipants)
                {
                    await _hubContext.Clients.User(participantId)
                        .SendAsync("MessageReceived", message);
                }

                // Send push notification for offline users
                await SendPushNotificationAsync(message, activeParticipants);

                _logger.LogInformation($"Message {message.Id} sent in conversation {dto.ConversationId}");
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> MarkMessageAsReadAsync(string messageId, string userId)
        {
            try
            {
                var message = await _messageRepository.GetAsync(messageId);
                if (message == null)
                    return false;

                var conversation = await _conversationRepository.GetAsync(message.ConversationId);
                var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                
                if (participant == null || !participant.IsActive)
                    return false;

                if (!message.ReadBy.Any(r => r.UserId == userId))
                {
                    message.ReadBy.Add(new MessageReadStatus
                    {
                        UserId = userId,
                        ReadAt = DateTime.UtcNow
                    });

                    message.IsRead = message.ReadBy.Count >= conversation.Participants.Count(p => p.IsActive) - 1;
                    await _messageRepository.UpdateAsync(message);

                    // Notify sender about read receipt
                    await _hubContext.Clients.User(message.SenderId)
                        .SendAsync("MessageRead", new { MessageId = messageId, ReadBy = userId, ReadAt = DateTime.UtcNow });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking message as read: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Message>> GetConversationMessagesAsync(string conversationId, string userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var conversation = await _conversationRepository.GetAsync(conversationId);
                if (conversation == null)
                    throw new ArgumentException("Conversation not found");

                var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null || !participant.IsActive)
                    throw new UnauthorizedAccessException("User is not a participant in this conversation");

                var messages = await _messageRepository.GetListAsync(
                    m => m.ConversationId == conversationId,
                    orderBy: q => q.OrderByDescending(m => m.SentAt),
                    skip: (page - 1) * pageSize,
                    take: pageSize
                );

                // Decrypt messages if needed
                if (conversation.Settings.IsEncrypted)
                {
                    foreach (var message in messages.Where(m => m.IsEncrypted))
                    {
                        message.Content = await DecryptMessageAsync(message.Content);
                    }
                }

                return messages.OrderBy(m => m.SentAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation messages: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId, ConversationFilter filter = null)
        {
            try
            {
                var conversations = await _conversationRepository.GetListAsync(
                    c => c.Participants.Any(p => p.UserId == userId && p.IsActive) &&
                         (filter?.Type == null || c.Type == filter.Type) &&
                         (filter?.IsActive == null || c.IsActive == filter.IsActive.Value),
                    orderBy: q => q.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt),
                    skip: filter != null && filter.Page > 0 ? (filter.Page - 1) * filter.PageSize : 0,
                    take: filter?.PageSize ?? 20
                );

                return conversations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user conversations: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AddParticipantAsync(string conversationId, string userId, string addedBy)
        {
            try
            {
                var conversation = await _conversationRepository.GetAsync(conversationId);
                if (conversation == null)
                    return false;

                var adder = conversation.Participants.FirstOrDefault(p => p.UserId == addedBy);
                if (adder == null || adder.Role != ParticipantRole.Admin)
                    return false;

                if (conversation.Participants.Any(p => p.UserId == userId))
                    return false;

                conversation.Participants.Add(new ConversationParticipant
                {
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow,
                    Role = ParticipantRole.Member,
                    IsActive = true
                });

                conversation.UpdatedAt = DateTime.UtcNow;
                await _conversationRepository.UpdateAsync(conversation);

                // Notify all participants
                foreach (var participant in conversation.Participants.Where(p => p.IsActive))
                {
                    await _hubContext.Clients.User(participant.UserId)
                        .SendAsync("ParticipantAdded", new { ConversationId = conversationId, UserId = userId, AddedBy = addedBy });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding participant: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveParticipantAsync(string conversationId, string userId, string removedBy)
        {
            try
            {
                var conversation = await _conversationRepository.GetAsync(conversationId);
                if (conversation == null)
                    return false;

                var remover = conversation.Participants.FirstOrDefault(p => p.UserId == removedBy);
                if (remover == null || (remover.Role != ParticipantRole.Admin && removedBy != userId))
                    return false;

                var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                    return false;

                participant.IsActive = false;
                participant.LeftAt = DateTime.UtcNow;

                conversation.UpdatedAt = DateTime.UtcNow;
                await _conversationRepository.UpdateAsync(conversation);

                // Notify all participants
                foreach (var activeParticipant in conversation.Participants.Where(p => p.IsActive))
                {
                    await _hubContext.Clients.User(activeParticipant.UserId)
                        .SendAsync("ParticipantRemoved", new { ConversationId = conversationId, UserId = userId, RemovedBy = removedBy });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing participant: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateTypingStatusAsync(string conversationId, string userId, bool isTyping)
        {
            try
            {
                var conversation = await _conversationRepository.GetAsync(conversationId);
                if (conversation == null)
                    return false;

                var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null || !participant.IsActive)
                    return false;

                // Notify other participants
                var otherParticipants = conversation.Participants
                    .Where(p => p.IsActive && p.UserId != userId)
                    .Select(p => p.UserId)
                    .ToList();

                foreach (var participantId in otherParticipants)
                {
                    await _hubContext.Clients.User(participantId)
                        .SendAsync("TypingStatusChanged", new { ConversationId = conversationId, UserId = userId, IsTyping = isTyping });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating typing status: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Message>> SearchMessagesAsync(string userId, MessageSearchCriteria criteria)
        {
            try
            {
                var userConversations = await _conversationRepository.GetListAsync(
                    c => c.Participants.Any(p => p.UserId == userId && p.IsActive)
                );

                var conversationIds = userConversations.Select(c => c.Id).ToList();

                var messages = await _messageRepository.GetListAsync(
                    m => conversationIds.Contains(m.ConversationId) &&
                         (string.IsNullOrEmpty(criteria.Query) || m.Content.Contains(criteria.Query)) &&
                         (criteria.ConversationId == null || m.ConversationId == criteria.ConversationId) &&
                         (criteria.SenderId == null || m.SenderId == criteria.SenderId) &&
                         (criteria.MessageType == null || m.Type == criteria.MessageType) &&
                         (criteria.StartDate == null || m.SentAt >= criteria.StartDate) &&
                         (criteria.EndDate == null || m.SentAt <= criteria.EndDate),
                    orderBy: q => q.OrderByDescending(m => m.SentAt),
                    take: criteria.MaxResults ?? 100
                );

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching messages: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteMessageAsync(string messageId, string userId)
        {
            try
            {
                var message = await _messageRepository.GetAsync(messageId);
                if (message == null)
                    return false;

                if (message.SenderId != userId)
                    return false;

                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;
                message.Content = "[Message deleted]";
                
                await _messageRepository.UpdateAsync(message);

                // Notify participants
                var conversation = await _conversationRepository.GetAsync(message.ConversationId);
                var activeParticipants = conversation.Participants
                    .Where(p => p.IsActive)
                    .Select(p => p.UserId)
                    .ToList();

                foreach (var participantId in activeParticipants)
                {
                    await _hubContext.Clients.User(participantId)
                        .SendAsync("MessageDeleted", new { MessageId = messageId, DeletedBy = userId });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message: {ex.Message}");
                return false;
            }
        }

        public async Task<Message> EditMessageAsync(string messageId, string userId, string newContent)
        {
            try
            {
                var message = await _messageRepository.GetAsync(messageId);
                if (message == null)
                    throw new ArgumentException("Message not found");

                if (message.SenderId != userId)
                    throw new UnauthorizedAccessException("Only the sender can edit the message");

                if (DateTime.UtcNow.Subtract(message.SentAt).TotalMinutes > 15)
                    throw new InvalidOperationException("Messages can only be edited within 15 minutes");

                var conversation = await _conversationRepository.GetAsync(message.ConversationId);
                
                message.Content = newContent;
                message.IsEdited = true;
                message.EditedAt = DateTime.UtcNow;

                // Encrypt if needed
                if (conversation.Settings.IsEncrypted)
                {
                    message.Content = await EncryptMessageAsync(message.Content);
                }

                await _messageRepository.UpdateAsync(message);

                // Notify participants
                var activeParticipants = conversation.Participants
                    .Where(p => p.IsActive)
                    .Select(p => p.UserId)
                    .ToList();

                foreach (var participantId in activeParticipants)
                {
                    await _hubContext.Clients.User(participantId)
                        .SendAsync("MessageEdited", message);
                }

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error editing message: {ex.Message}");
                throw;
            }
        }

        public async Task<MessagingAnalytics> GetMessagingAnalyticsAsync(string userId, MessagingAnalyticsFilter filter)
        {
            try
            {
                var userConversations = await _conversationRepository.GetListAsync(
                    c => c.Participants.Any(p => p.UserId == userId && p.IsActive)
                );

                var conversationIds = userConversations.Select(c => c.Id).ToList();

                var messages = await _messageRepository.GetListAsync(
                    m => conversationIds.Contains(m.ConversationId) &&
                         (filter.StartDate == null || m.SentAt >= filter.StartDate) &&
                         (filter.EndDate == null || m.SentAt <= filter.EndDate)
                );

                var analytics = new MessagingAnalytics
                {
                    TotalConversations = userConversations.Count,
                    ActiveConversations = userConversations.Count(c => c.IsActive),
                    TotalMessages = messages.Count,
                    MessagesSent = messages.Count(m => m.SenderId == userId),
                    MessagesReceived = messages.Count(m => m.SenderId != userId),
                    AverageResponseTime = CalculateAverageResponseTime(messages, userId),
                    MessageTypeBreakdown = messages.GroupBy(m => m.Type)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    ConversationTypeBreakdown = userConversations.GroupBy(c => c.Type)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    PeriodStart = filter.StartDate ?? DateTime.UtcNow.AddDays(-30),
                    PeriodEnd = filter.EndDate ?? DateTime.UtcNow
                };

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messaging analytics: {ex.Message}");
                throw;
            }
        }

        private async Task<string> EncryptMessageAsync(string content)
        {
            // Implement encryption logic here
            // For now, return base64 encoded content as placeholder
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(bytes);
        }

        private async Task<string> DecryptMessageAsync(string encryptedContent)
        {
            // Implement decryption logic here
            // For now, return base64 decoded content as placeholder
            try
            {
                var bytes = Convert.FromBase64String(encryptedContent);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return encryptedContent; // Return as-is if decryption fails
            }
        }

        private async Task SendPushNotificationAsync(Message message, List<string> recipientIds)
        {
            try
            {
                var sender = await _userRepository.GetAsync(message.SenderId);
                var notification = new CreateNotificationDto
                {
                    Title = $"New message from {sender?.Name ?? "Unknown"}",
                    Body = message.Content.Length > 100 ? message.Content.Substring(0, 100) + "..." : message.Content,
                    Type = NotificationType.Message,
                    Data = new Dictionary<string, object>
                    {
                        ["conversationId"] = message.ConversationId,
                        ["messageId"] = message.Id,
                        ["senderId"] = message.SenderId
                    },
                    RecipientIds = recipientIds
                };

                await _notificationService.SendNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending push notification: {ex.Message}");
            }
        }

        private double CalculateAverageResponseTime(List<Message> messages, string userId)
        {
            var responseTimes = new List<double>();
            var sortedMessages = messages.OrderBy(m => m.SentAt).ToList();

            for (int i = 1; i < sortedMessages.Count; i++)
            {
                var currentMessage = sortedMessages[i];
                var previousMessage = sortedMessages[i - 1];

                if (currentMessage.SenderId == userId && previousMessage.SenderId != userId)
                {
                    var responseTime = currentMessage.SentAt.Subtract(previousMessage.SentAt).TotalMinutes;
                    if (responseTime <= 1440) // Only consider responses within 24 hours
                    {
                        responseTimes.Add(responseTime);
                    }
                }
            }

            return responseTimes.Any() ? responseTimes.Average() : 0;
        }
    }

    public class MessagingHub : Hub
    {
        private readonly ILogger<MessagingHub> _logger;

        public MessagingHub(ILogger<MessagingHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogInformation($"User {Context.UserIdentifier} joined conversation {conversationId}");
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogInformation($"User {Context.UserIdentifier} left conversation {conversationId}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"User {Context.UserIdentifier} connected to messaging hub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"User {Context.UserIdentifier} disconnected from messaging hub");
            await base.OnDisconnectedAsync(exception);
        }
    }
}

