using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Support ticket DTO
    /// </summary>
    public class SupportTicket
    {
        public string Id { get; set; }
        public string TicketNumber { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public TicketCategory Category { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public string AssignedAgentId { get; set; }
        public string AssignedAgentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string ClosedBy { get; set; }
        public string CloseReason { get; set; }
        public List<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
        public List<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public int ResponseTime { get; set; } // in minutes
        public int ResolutionTime { get; set; } // in minutes
        public CustomerSatisfactionRating? SatisfactionRating { get; set; }
        public string SatisfactionFeedback { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>
    /// Create support ticket DTO
    /// </summary>
    public class CreateSupportTicketDto
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Subject { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Description { get; set; }
        
        [Required]
        public TicketCategory Category { get; set; }
        
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        
        public List<CreateTicketAttachmentDto> Attachments { get; set; } = new List<CreateTicketAttachmentDto>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>
    /// Update support ticket DTO
    /// </summary>
    public class UpdateSupportTicketDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public TicketCategory? Category { get; set; }
        public TicketPriority? Priority { get; set; }
        public TicketStatus? Status { get; set; }
        public string AssignedAgentId { get; set; }
        public List<string> Tags { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// Ticket message DTO
    /// </summary>
    public class TicketMessage
    {
        public string Id { get; set; }
        public string TicketId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public MessageSenderType SenderType { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsInternal { get; set; }
        public List<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Create ticket message DTO
    /// </summary>
    public class CreateTicketMessageDto
    {
        [Required]
        public string SenderId { get; set; }
        
        [Required]
        public MessageSenderType SenderType { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
        
        public MessageType Type { get; set; } = MessageType.Text;
        
        public bool IsInternal { get; set; } = false;
        
        public List<CreateMessageAttachmentDto> Attachments { get; set; } = new List<CreateMessageAttachmentDto>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Chat session DTO
    /// </summary>
    public class ChatSession
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string AssignedAgentId { get; set; }
        public string AssignedAgentName { get; set; }
        public ChatSessionStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string EndedBy { get; set; }
        public int MessageCount { get; set; }
        public TimeSpan Duration { get; set; }
        public ChatCategory Category { get; set; }
        public ChatPriority Priority { get; set; }
        public CustomerSatisfactionRating? SatisfactionRating { get; set; }
        public string SatisfactionFeedback { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>
    /// Start chat session DTO
    /// </summary>
    public class StartChatSessionDto
    {
        [Required]
        public string UserId { get; set; }
        
        public ChatCategory Category { get; set; } = ChatCategory.General;
        
        public ChatPriority Priority { get; set; } = ChatPriority.Normal;
        
        public string InitialMessage { get; set; }
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>
    /// Chat message DTO
    /// </summary>
    public class ChatMessage
    {
        public string Id { get; set; }
        public string SessionId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public MessageSenderType SenderType { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public List<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Send chat message DTO
    /// </summary>
    public class SendChatMessageDto
    {
        [Required]
        public string SenderId { get; set; }
        
        [Required]
        public MessageSenderType SenderType { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
        
        public MessageType Type { get; set; } = MessageType.Text;
        
        public List<CreateMessageAttachmentDto> Attachments { get; set; } = new List<CreateMessageAttachmentDto>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Support agent DTO
    /// </summary>
    public class SupportAgent
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public AgentRole Role { get; set; }
        public AgentStatus Status { get; set; }
        public AgentAvailabilityStatus AvailabilityStatus { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
        public int MaxConcurrentChats { get; set; } = 5;
        public int MaxConcurrentTickets { get; set; } = 20;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActiveAt { get; set; }
        public AgentPerformanceMetrics PerformanceMetrics { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Create support agent DTO
    /// </summary>
    public class CreateSupportAgentDto
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        [Required]
        public string Department { get; set; }
        
        [Required]
        public AgentRole Role { get; set; }
        
        public List<string> Skills { get; set; } = new List<string>();
        
        public List<string> Languages { get; set; } = new List<string>();
        
        public int MaxConcurrentChats { get; set; } = 5;
        
        public int MaxConcurrentTickets { get; set; } = 20;
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Update support agent DTO
    /// </summary>
    public class UpdateSupportAgentDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public AgentRole? Role { get; set; }
        public AgentStatus? Status { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Languages { get; set; }
        public int? MaxConcurrentChats { get; set; }
        public int? MaxConcurrentTickets { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// FAQ item DTO
    /// </summary>
    public class FaqItem
    {
        public string Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public int ViewCount { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
        public double HelpfulnessRatio { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Dictionary<string, LocalizedFaqContent> LocalizedContent { get; set; } = new Dictionary<string, LocalizedFaqContent>();
        public List<string> RelatedFaqIds { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Create FAQ item DTO
    /// </summary>
    public class CreateFaqItemDto
    {
        [Required]
        [StringLength(500)]
        public string Question { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Answer { get; set; }
        
        [Required]
        public string CategoryId { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        
        public bool IsPublished { get; set; } = true;
        
        public Dictionary<string, LocalizedFaqContent> LocalizedContent { get; set; } = new Dictionary<string, LocalizedFaqContent>();
        
        public List<string> RelatedFaqIds { get; set; } = new List<string>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Update FAQ item DTO
    /// </summary>
    public class UpdateFaqItemDto
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public bool? IsPublished { get; set; }
        public Dictionary<string, LocalizedFaqContent> LocalizedContent { get; set; }
        public List<string> RelatedFaqIds { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// FAQ category DTO
    /// </summary>
    public class FaqCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int FaqCount { get; set; }
        public Dictionary<string, string> LocalizedNames { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> LocalizedDescriptions { get; set; } = new Dictionary<string, string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Knowledge base article DTO
    /// </summary>
    public class KnowledgeBaseArticle
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public ArticleStatus Status { get; set; }
        public int ViewCount { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
        public double HelpfulnessRatio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public Dictionary<string, LocalizedArticleContent> LocalizedContent { get; set; } = new Dictionary<string, LocalizedArticleContent>();
        public List<string> RelatedArticleIds { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Create knowledge base article DTO
    /// </summary>
    public class CreateKnowledgeBaseArticleDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [StringLength(500)]
        public string Summary { get; set; }
        
        [Required]
        public string CategoryId { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        
        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
        
        public Dictionary<string, LocalizedArticleContent> LocalizedContent { get; set; } = new Dictionary<string, LocalizedArticleContent>();
        
        public List<string> RelatedArticleIds { get; set; } = new List<string>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Update knowledge base article DTO
    /// </summary>
    public class UpdateKnowledgeBaseArticleDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public ArticleStatus? Status { get; set; }
        public Dictionary<string, LocalizedArticleContent> LocalizedContent { get; set; }
        public List<string> RelatedArticleIds { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// Supporting enums and classes
    /// </summary>
    public enum TicketCategory
    {
        General,
        OrderIssue,
        PaymentProblem,
        DeliveryIssue,
        AccountProblem,
        TechnicalSupport,
        FeatureRequest,
        Complaint,
        Compliment,
        RefundRequest,
        Other
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Emergency
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        Pending,
        Resolved,
        Closed,
        Reopened,
        Escalated
    }

    public enum MessageSenderType
    {
        Customer,
        Agent,
        System,
        Bot
    }

    #if false // Duplicate of MessagingDtos.MessageType
    public enum MessageType
    {
        Text,
        Image,
        File,
        Audio,
        Video,
        System,
        Template
    }
    #endif

    public enum ChatSessionStatus
    {
        Waiting,
        Active,
        Ended,
        Transferred,
        Abandoned
    }

    public enum ChatCategory
    {
        General,
        OrderSupport,
        PaymentHelp,
        TechnicalIssue,
        AccountHelp,
        Complaint,
        Other
    }

    public enum ChatPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public enum AgentRole
    {
        Agent,
        SeniorAgent,
        TeamLead,
        Supervisor,
        Manager,
        Administrator
    }

    public enum AgentStatus
    {
        Active,
        Inactive,
        Suspended,
        Training
    }

    public enum AgentAvailabilityStatus
    {
        Available,
        Busy,
        Away,
        DoNotDisturb,
        Offline
    }

    public enum CustomerSatisfactionRating
    {
        VeryDissatisfied = 1,
        Dissatisfied = 2,
        Neutral = 3,
        Satisfied = 4,
        VerySatisfied = 5
    }

    public enum ArticleStatus
    {
        Draft,
        Review,
        Published,
        Archived
    }

    public class LocalizedFaqContent
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class LocalizedArticleContent
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
    }

    public class TicketAttachment
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; }
    }

    public class CreateTicketAttachmentDto
    {
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string FileUrl { get; set; }
        
        [Required]
        public string FileType { get; set; }
        
        public long FileSize { get; set; }
    }

    #if false // Duplicate of MessagingDtos.MessageAttachment and CreateMessageAttachmentDto
    public class MessageAttachment
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string ThumbnailUrl { get; set; }
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
    }
    #endif

    public class TicketFilter
    {
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public TicketCategory? Category { get; set; }
        public string AssignedAgentId { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "DESC";
    }

    public class TicketSearchCriteria
    {
        public string Query { get; set; }
        public List<string> SearchFields { get; set; } = new List<string> { "Subject", "Description" };
        public TicketFilter Filter { get; set; }
    }

    public class TicketStatistics
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public double AverageResponseTime { get; set; } // in minutes
        public double AverageResolutionTime { get; set; } // in minutes
        public double CustomerSatisfactionScore { get; set; }
        public Dictionary<TicketCategory, int> CategoryBreakdown { get; set; } = new Dictionary<TicketCategory, int>();
        public Dictionary<TicketPriority, int> PriorityBreakdown { get; set; } = new Dictionary<TicketPriority, int>();
        public Dictionary<string, int> AgentWorkload { get; set; } = new Dictionary<string, int>();
    }

    public class TicketStatisticsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AgentId { get; set; }
        public string Department { get; set; }
        public List<TicketCategory> Categories { get; set; } = new List<TicketCategory>();
        public List<TicketPriority> Priorities { get; set; } = new List<TicketPriority>();
    }

    public class TicketHistoryEntry
    {
        public string Id { get; set; }
        public string TicketId { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string PerformedBy { get; set; }
        public string PerformedByName { get; set; }
        public DateTime PerformedAt { get; set; }
        public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();
    }

    public class AgentPerformanceMetrics
    {
        public string AgentId { get; set; }
        public int TicketsHandled { get; set; }
        public int TicketsResolved { get; set; }
        public int ChatsHandled { get; set; }
        public double AverageResponseTime { get; set; } // in minutes
        public double AverageResolutionTime { get; set; } // in minutes
        public double CustomerSatisfactionScore { get; set; }
        public int TotalWorkingHours { get; set; }
        public double ProductivityScore { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class AgentWorkload
    {
        public string AgentId { get; set; }
        public int ActiveTickets { get; set; }
        public int ActiveChats { get; set; }
        public int PendingTickets { get; set; }
        public double WorkloadPercentage { get; set; }
        public AgentAvailabilityStatus AvailabilityStatus { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class AgentSchedule
    {
        public string Id { get; set; }
        public string AgentId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string TimeZone { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateAgentScheduleDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class ChatAnalytics
    {
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int AbandonedSessions { get; set; }
        public double AverageWaitTime { get; set; } // in minutes
        public double AverageSessionDuration { get; set; } // in minutes
        public double CustomerSatisfactionScore { get; set; }
        public Dictionary<ChatCategory, int> CategoryBreakdown { get; set; } = new Dictionary<ChatCategory, int>();
        public Dictionary<string, int> AgentPerformance { get; set; } = new Dictionary<string, int>();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class ChatAnalyticsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AgentId { get; set; }
        public List<ChatCategory> Categories { get; set; } = new List<ChatCategory>();
        public List<ChatSessionStatus> Statuses { get; set; } = new List<ChatSessionStatus>();
    }

    public class FaqFilter
    {
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool? IsPublished { get; set; }
        public string Language { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "ViewCount";
        public string SortOrder { get; set; } = "DESC";
    }

    public class FaqAnalytics
    {
        public int TotalFaqs { get; set; }
        public int TotalViews { get; set; }
        public int TotalHelpfulVotes { get; set; }
        public int TotalNotHelpfulVotes { get; set; }
        public double AverageHelpfulnessRatio { get; set; }
        public List<FaqItem> MostViewedFaqs { get; set; } = new List<FaqItem>();
        public List<FaqItem> MostHelpfulFaqs { get; set; } = new List<FaqItem>();
        public List<FaqItem> LeastHelpfulFaqs { get; set; } = new List<FaqItem>();
        public Dictionary<string, int> CategoryBreakdown { get; set; } = new Dictionary<string, int>();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class FaqAnalyticsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string Language { get; set; }
    }

    public class AutomatedResponse
    {
        public string Id { get; set; }
        public string Query { get; set; }
        public string Response { get; set; }
        public double ConfidenceScore { get; set; }
        public string TemplateId { get; set; }
        public MessageIntent Intent { get; set; }
        public List<SuggestedAction> SuggestedActions { get; set; } = new List<SuggestedAction>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ResponseTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public List<string> Triggers { get; set; } = new List<string>();
        public MessageIntent Intent { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Dictionary<string, string> LocalizedContent { get; set; } = new Dictionary<string, string>();
    }

    public class CreateResponseTemplateDto
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public List<string> Triggers { get; set; } = new List<string>();
        
        public MessageIntent Intent { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public Dictionary<string, string> LocalizedContent { get; set; } = new Dictionary<string, string>();
    }

    public class UpdateResponseTemplateDto
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public List<string> Triggers { get; set; }
        public MessageIntent? Intent { get; set; }
        public bool? IsActive { get; set; }
        public Dictionary<string, string> LocalizedContent { get; set; }
    }

    public class MessageIntent
    {
        public string Intent { get; set; }
        public double Confidence { get; set; }
        public List<string> Entities { get; set; } = new List<string>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class SuggestedResponse
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public double RelevanceScore { get; set; }
        public string TemplateId { get; set; }
        public string Source { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class SuggestedAction
    {
        public string Action { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class TrainingData
    {
        public string Query { get; set; }
        public string ExpectedResponse { get; set; }
        public MessageIntent Intent { get; set; }
        public string Language { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class AutomationAnalytics
    {
        public int TotalQueries { get; set; }
        public int AutomatedResponses { get; set; }
        public int ManualResponses { get; set; }
        public double AutomationRate { get; set; }
        public double AverageConfidenceScore { get; set; }
        public double CustomerSatisfactionScore { get; set; }
        public Dictionary<string, int> IntentBreakdown { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TemplateUsage { get; set; } = new Dictionary<string, int>();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class AutomationAnalyticsFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Intents { get; set; } = new List<string>();
        public List<string> TemplateIds { get; set; } = new List<string>();
        public string Language { get; set; }
        public double? MinConfidenceScore { get; set; }
    }
}

