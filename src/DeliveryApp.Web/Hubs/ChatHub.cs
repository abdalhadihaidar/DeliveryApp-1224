using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DeliveryApp.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinChatSession(string sessionId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatSession_{sessionId}");
                _logger.LogInformation($"User {Context.User.Identity.Name} joined chat session {sessionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to join chat session {sessionId}");
            }
        }

        public async Task LeaveChatSession(string sessionId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatSession_{sessionId}");
                _logger.LogInformation($"User {Context.User.Identity.Name} left chat session {sessionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to leave chat session {sessionId}");
            }
        }

        public async Task SendMessage(string sessionId, string message, string senderType)
        {
            try
            {
                await Clients.Group($"ChatSession_{sessionId}").SendAsync("ReceiveMessage", new
                {
                    SessionId = sessionId,
                    Message = message,
                    SenderType = senderType,
                    Timestamp = DateTime.UtcNow,
                    SenderId = Context.UserIdentifier
                });
                
                _logger.LogInformation($"Message sent to chat session {sessionId} by {Context.User.Identity.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send message to chat session {sessionId}");
            }
        }

        public async Task MarkAsRead(string sessionId, string userId)
        {
            try
            {
                await Clients.Group($"ChatSession_{sessionId}").SendAsync("MessagesRead", new
                {
                    SessionId = sessionId,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation($"Messages marked as read in session {sessionId} by user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark messages as read in session {sessionId}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"User {Context.User.Identity.Name} connected to ChatHub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"User {Context.User.Identity.Name} disconnected from ChatHub");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
