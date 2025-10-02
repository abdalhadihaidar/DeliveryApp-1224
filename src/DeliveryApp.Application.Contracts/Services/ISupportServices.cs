using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Interface for customer support ticket system
    /// </summary>
    [RemoteService]
    public interface ISupportTicketService : IApplicationService
    {
        /// <summary>
        /// Create a new support ticket
        /// </summary>
        Task<SupportTicket> CreateTicketAsync(CreateSupportTicketDto ticket);

        /// <summary>
        /// Update support ticket
        /// </summary>
        Task<SupportTicket> UpdateTicketAsync(string ticketId, UpdateSupportTicketDto ticket);

        /// <summary>
        /// Get support ticket by ID
        /// </summary>
        Task<SupportTicket> GetTicketAsync(string ticketId);

        /// <summary>
        /// Get tickets for a user
        /// </summary>
        Task<List<SupportTicket>> GetUserTicketsAsync(string userId, TicketFilter filter = null);

        /// <summary>
        /// Get tickets assigned to an agent
        /// </summary>
        Task<List<SupportTicket>> GetAgentTicketsAsync(string agentId, TicketFilter filter = null);

        /// <summary>
        /// Assign ticket to an agent
        /// </summary>
        Task<bool> AssignTicketAsync(string ticketId, string agentId, string assignedBy);

        /// <summary>
        /// Add message to ticket
        /// </summary>
        Task<TicketMessage> AddTicketMessageAsync(string ticketId, CreateTicketMessageDto message);

        /// <summary>
        /// Close support ticket
        /// </summary>
        Task<bool> CloseTicketAsync(string ticketId, string closedBy, string reason);

        /// <summary>
        /// Reopen support ticket
        /// </summary>
        Task<bool> ReopenTicketAsync(string ticketId, string reopenedBy, string reason);

        /// <summary>
        /// Escalate ticket
        /// </summary>
        Task<bool> EscalateTicketAsync(string ticketId, TicketPriority newPriority, string escalatedBy, string reason);

        /// <summary>
        /// Get ticket statistics
        /// </summary>
        Task<TicketStatistics> GetTicketStatisticsAsync(TicketStatisticsFilter filter);

        /// <summary>
        /// Search tickets
        /// </summary>
        Task<List<SupportTicket>> SearchTicketsAsync(TicketSearchCriteria criteria);

        /// <summary>
        /// Get ticket history
        /// </summary>
        Task<List<TicketHistoryEntry>> GetTicketHistoryAsync(string ticketId);
    }

    /// <summary>
    /// Interface for live chat service
    /// </summary>
    [RemoteService]
    public interface ILiveChatService : IApplicationService
    {
        /// <summary>
        /// Start a new chat session
        /// </summary>
        Task<ChatSession> StartChatSessionAsync(StartChatSessionDto session);

        /// <summary>
        /// End chat session
        /// </summary>
        Task<bool> EndChatSessionAsync(string sessionId, string endedBy);

        /// <summary>
        /// Send chat message
        /// </summary>
        Task<ChatMessage> SendMessageAsync(string sessionId, SendChatMessageDto message);

        /// <summary>
        /// Get chat session
        /// </summary>
        Task<ChatSession> GetChatSessionAsync(string sessionId);

        /// <summary>
        /// Get chat messages
        /// </summary>
        Task<List<ChatMessage>> GetChatMessagesAsync(string sessionId, int page = 1, int pageSize = 50);

        /// <summary>
        /// Get active chat sessions for agent
        /// </summary>
        Task<List<ChatSession>> GetAgentChatSessionsAsync(string agentId);

        /// <summary>
        /// Get user chat sessions
        /// </summary>
        Task<List<ChatSession>> GetUserChatSessionsAsync(string userId);

        /// <summary>
        /// Assign chat session to agent
        /// </summary>
        Task<bool> AssignChatSessionAsync(string sessionId, string agentId);

        /// <summary>
        /// Transfer chat session to another agent
        /// </summary>
        Task<bool> TransferChatSessionAsync(string sessionId, string fromAgentId, string toAgentId, string reason);

        /// <summary>
        /// Set agent availability
        /// </summary>
        Task<bool> SetAgentAvailabilityAsync(string agentId, AgentAvailabilityStatus status);

        /// <summary>
        /// Get available agents
        /// </summary>
        Task<List<SupportAgent>> GetAvailableAgentsAsync();

        /// <summary>
        /// Get chat analytics
        /// </summary>
        Task<ChatAnalytics> GetChatAnalyticsAsync(ChatAnalyticsFilter filter);
    }

    /// <summary>
    /// Interface for FAQ and knowledge base service
    /// </summary>
    [RemoteService]
    public interface IKnowledgeBaseService : IApplicationService
    {
        /// <summary>
        /// Create FAQ item
        /// </summary>
        Task<FaqItem> CreateFaqItemAsync(CreateFaqItemDto faq);

        /// <summary>
        /// Update FAQ item
        /// </summary>
        Task<FaqItem> UpdateFaqItemAsync(string faqId, UpdateFaqItemDto faq);

        /// <summary>
        /// Delete FAQ item
        /// </summary>
        Task<bool> DeleteFaqItemAsync(string faqId);

        /// <summary>
        /// Get FAQ item by ID
        /// </summary>
        Task<FaqItem> GetFaqItemAsync(string faqId);

        /// <summary>
        /// Get all FAQ items
        /// </summary>
        Task<List<FaqItem>> GetAllFaqItemsAsync(FaqFilter filter = null);

        /// <summary>
        /// Search FAQ items
        /// </summary>
        Task<List<FaqItem>> SearchFaqItemsAsync(string query, string language = "en");

        /// <summary>
        /// Get FAQ categories
        /// </summary>
        Task<List<FaqCategory>> GetFaqCategoriesAsync();

        /// <summary>
        /// Create knowledge base article
        /// </summary>
        Task<KnowledgeBaseArticle> CreateArticleAsync(CreateKnowledgeBaseArticleDto article);

        /// <summary>
        /// Update knowledge base article
        /// </summary>
        Task<KnowledgeBaseArticle> UpdateArticleAsync(string articleId, UpdateKnowledgeBaseArticleDto article);

        /// <summary>
        /// Get knowledge base article
        /// </summary>
        Task<KnowledgeBaseArticle> GetArticleAsync(string articleId);

        /// <summary>
        /// Search knowledge base articles
        /// </summary>
        Task<List<KnowledgeBaseArticle>> SearchArticlesAsync(string query, string language = "en");

        /// <summary>
        /// Track FAQ usage
        /// </summary>
        Task TrackFaqUsageAsync(string faqId, string userId);

        /// <summary>
        /// Get FAQ analytics
        /// </summary>
        Task<FaqAnalytics> GetFaqAnalyticsAsync(FaqAnalyticsFilter filter);
    }

    /// <summary>
    /// Interface for support agent management
    /// </summary>
    [RemoteService]
    public interface ISupportAgentService : IApplicationService
    {
        /// <summary>
        /// Create support agent
        /// </summary>
        Task<SupportAgent> CreateAgentAsync(CreateSupportAgentDto agent);

        /// <summary>
        /// Update support agent
        /// </summary>
        Task<SupportAgent> UpdateAgentAsync(string agentId, UpdateSupportAgentDto agent);

        /// <summary>
        /// Get support agent
        /// </summary>
        Task<SupportAgent> GetAgentAsync(string agentId);

        /// <summary>
        /// Get all support agents
        /// </summary>
        Task<List<SupportAgent>> GetAllAgentsAsync();

        /// <summary>
        /// Set agent status
        /// </summary>
        Task<bool> SetAgentStatusAsync(string agentId, AgentStatus status);

        /// <summary>
        /// Get agent performance metrics
        /// </summary>
        Task<AgentPerformanceMetrics> GetAgentPerformanceAsync(string agentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get agent workload
        /// </summary>
        Task<AgentWorkload> GetAgentWorkloadAsync(string agentId);

        /// <summary>
        /// Assign agent to department
        /// </summary>
        Task<bool> AssignAgentToDepartmentAsync(string agentId, string departmentId);

        /// <summary>
        /// Get agent schedule
        /// </summary>
        Task<List<AgentSchedule>> GetAgentScheduleAsync(string agentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Update agent schedule
        /// </summary>
        Task<bool> UpdateAgentScheduleAsync(string agentId, List<UpdateAgentScheduleDto> schedule);
    }

    /// <summary>
    /// Interface for automated support responses
    /// </summary>
    [RemoteService]
    public interface IAutomatedSupportService : IApplicationService
    {
        /// <summary>
        /// Generate automated response
        /// </summary>
        Task<AutomatedResponse> GenerateResponseAsync(string query, string language = "en");

        /// <summary>
        /// Create response template
        /// </summary>
        Task<ResponseTemplate> CreateResponseTemplateAsync(CreateResponseTemplateDto template);

        /// <summary>
        /// Update response template
        /// </summary>
        Task<ResponseTemplate> UpdateResponseTemplateAsync(string templateId, UpdateResponseTemplateDto template);

        /// <summary>
        /// Get response templates
        /// </summary>
        Task<List<ResponseTemplate>> GetResponseTemplatesAsync();

        /// <summary>
        /// Analyze message intent
        /// </summary>
        Task<MessageIntent> AnalyzeMessageIntentAsync(string message, string language = "en");

        /// <summary>
        /// Get suggested responses
        /// </summary>
        Task<List<SuggestedResponse>> GetSuggestedResponsesAsync(string ticketId);

        /// <summary>
        /// Train automated response system
        /// </summary>
        Task<bool> TrainResponseSystemAsync(List<TrainingData> trainingData);

        /// <summary>
        /// Get automation analytics
        /// </summary>
        Task<AutomationAnalytics> GetAutomationAnalyticsAsync(AutomationAnalyticsFilter filter);
    }
}

