using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Web.Controllers
{
    [Route("api/chat")]
    [Authorize]
    public class ChatController : AbpControllerBase
    {
        private readonly IChatAppService _chatAppService;

        public ChatController(IChatAppService chatAppService)
        {
            _chatAppService = chatAppService;
        }

        [HttpPost("start-session")]
        public async Task<ActionResult<ChatSessionDto>> StartChatSession([FromBody] StartDeliveryChatSessionDto input)
        {
            var session = await _chatAppService.StartChatSessionAsync(input);
            return Ok(session);
        }

        [HttpPost("send-message")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendDeliveryChatMessageDto input)
        {
            var message = await _chatAppService.SendMessageAsync(input);
            return Ok(message);
        }

        [HttpGet("session/{sessionId}/messages")]
        public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(Guid sessionId)
        {
            var messages = await _chatAppService.GetMessagesAsync(sessionId);
            return Ok(messages);
        }

        [HttpPost("mark-as-read")]
        public async Task<ActionResult> MarkMessagesAsRead([FromBody] MarkMessagesAsReadDto input)
        {
            await _chatAppService.MarkMessagesAsReadAsync(input);
            return Ok();
        }

        [HttpGet("delivery/{deliveryId}/session")]
        public async Task<ActionResult<ChatSessionDto>> GetChatSessionByDeliveryId(Guid deliveryId)
        {
            var session = await _chatAppService.GetChatSessionByDeliveryIdAsync(deliveryId);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }

        [HttpGet("session/{sessionId}/with-messages")]
        public async Task<ActionResult<ChatSessionWithMessagesDto>> GetChatSessionWithMessages(Guid sessionId)
        {
            var session = await _chatAppService.GetChatSessionWithMessagesAsync(sessionId);
            return Ok(session);
        }

        [HttpGet("active-sessions")]
        [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<List<ChatSessionDto>>> GetActiveChatSessions()
        {
            var sessions = await _chatAppService.GetActiveChatSessionsAsync();
            return Ok(sessions);
        }

        [HttpGet("messages/new")]
        public async Task<ActionResult<List<ChatMessageDto>>> GetNewMessages([FromQuery] string sessionId, [FromQuery] string since)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return BadRequest("Invalid session ID format");
            }

            if (!DateTime.TryParse(since, out var sinceDate))
            {
                return BadRequest("Invalid since date format");
            }

            var messages = await _chatAppService.GetNewMessagesAsync(sessionGuid, sinceDate);
            return Ok(messages);
        }

        [HttpPost("notify/mobile")]
        public async Task<ActionResult> NotifyMobileApp([FromBody] object notification)
        {
            // This endpoint is for mobile app notifications
            // For now, just return OK as it's not critical for dashboard functionality
            return Ok();
        }

   
        [HttpPost("sessions/{sessionId}/close")]
        public async Task<ActionResult> CloseChatSession(Guid sessionId)
        {
            await _chatAppService.CloseChatSessionAsync(sessionId);
            return Ok();
        }

        [HttpGet("sessions/history")]
        public async Task<ActionResult<List<ChatSessionDto>>> GetChatHistory([FromQuery] string deliveryId)
        {
            if (!Guid.TryParse(deliveryId, out var deliveryGuid))
            {
                return BadRequest("Invalid delivery ID format");
            }

            var sessions = await _chatAppService.GetChatHistoryAsync(deliveryGuid);
            return Ok(sessions);
        }
    }
}
