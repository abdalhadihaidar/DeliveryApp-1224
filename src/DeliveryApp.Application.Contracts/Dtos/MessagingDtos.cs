using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class Conversation
    {
        public string Id { get; set; }
        public ConversationType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string LastMessageId { get; set; }
        public bool IsActive { get; set; }
        public List<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ConversationSettings Settings { get; set; } = new ConversationSettings();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class CreateConversationDto
    {
        [Required]
        public ConversationType Type { get; set; }
        
        [StringLength(200)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public string CreatedBy { get; set; }
        
        [Required]
        public List<string> ParticipantIds { get; set; } = new List<string>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class Message
    {
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsEncrypted { get; set; }
        public List<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
        public List<MessageReadStatus> ReadBy { get; set; } = new List<MessageReadStatus>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public string ReplyToMessageId { get; set; }
        public List<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    }

    public class SendMessageDto
    {
        [Required]
        public string ConversationId { get; set; }
        
        [Required]
        public string SenderId { get; set; }
        
        [Required]
        [StringLength(4000)]
        public string Content { get; set; }
        
        public MessageType Type { get; set; } = MessageType.Text;
        
        public List<CreateMessageAttachmentDto> Attachments { get; set; } = new List<CreateMessageAttachmentDto>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        public string ReplyToMessageId { get; set; }
    }

    public class ConversationParticipant
    {
        public string UserId { get; set; }
        public ParticipantRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool IsTyping { get; set; }
        public DateTime? LastTypingAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ConversationSettings
    {
        public bool AllowFileSharing { get; set; } = true;
        public bool AllowMediaSharing { get; set; } = true;
        public bool IsEncrypted { get; set; } = false;
        public int MessageRetentionDays { get; set; } = 365;
        public bool AllowMessageEditing { get; set; } = true;
        public bool AllowMessageDeletion { get; set; } = true;
        public bool ShowReadReceipts { get; set; } = true;
        public bool ShowTypingIndicators { get; set; } = true;
        public long MaxFileSize { get; set; } = 10485760; // 10MB
        public List<string> AllowedFileTypes { get; set; } = new List<string> { "jpg", "jpeg", "png", "gif", "pdf", "doc", "docx" };
    }

    public class MessageAttachment
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string ThumbnailUrl { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; } // for audio/video files
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class CreateMessageAttachmentDto
    {
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string FileUrl { get; set; }
        
        [Required]
        public string FileType { get; set; }
        
        public long FileSize { get; set; }
        
        public string ThumbnailUrl { get; set; }
        
        public int? Width { get; set; }
        
        public int? Height { get; set; }
        
        public int? Duration { get; set; }
    }

    public class MessageReadStatus
    {
        public string UserId { get; set; }
        public DateTime ReadAt { get; set; }
    }

    public class MessageReaction
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Emoji { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationFilter
    {
        public ConversationType? Type { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? LastMessageAfter { get; set; }
        public DateTime? LastMessageBefore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "LastMessageAt";
        public string SortOrder { get; set; } = "DESC";
    }

    public class MessageSearchCriteria
    {
        public string Query { get; set; }
        public string ConversationId { get; set; }
        public string SenderId { get; set; }
        public MessageType? MessageType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? HasAttachments { get; set; }
        public int? MaxResults { get; set; } = 100;
    }

    public class MessagingAnalytics
    {
        public int TotalConversations { get; set; }
        public int ActiveConversations { get; set; }
        public int TotalMessages { get; set; }
        public int MessagesSent { get; set; }
        public int MessagesReceived { get; set; }
        public double AverageResponseTime { get; set; } // in minutes
        public Dictionary<string, int> MessageTypeBreakdown { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ConversationTypeBreakdown { get; set; } = new Dictionary<string, int>();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class MessagingAnalyticsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ConversationType? ConversationType { get; set; }
        public List<string> ConversationIds { get; set; } = new List<string>();
    }

    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool IsOnline { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public enum ConversationType
    {
        DirectMessage,
        GroupChat,
        CustomerSupport,
        OrderChat,
        RestaurantChat,
        DeliveryChat,
        Announcement
    }

    public enum MessageType
    {
        Text,
        Image,
        Video,
        Audio,
        File,
        Location,
        Contact,
        System,
        Sticker,
        GIF
    }

    public enum ParticipantRole
    {
        Member,
        Admin,
        Moderator,
        Owner
    }

    public enum UserStatus
    {
        Online,
        Away,
        Busy,
        Offline,
        DoNotDisturb
    }

  

    public class CreateNotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public List<string> RecipientIds { get; set; } = new List<string>();
        public DateTime? ScheduledAt { get; set; }
        public string ImageUrl { get; set; }
        public string ActionUrl { get; set; }
    }
}

