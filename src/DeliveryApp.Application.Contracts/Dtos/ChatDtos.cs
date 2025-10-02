using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class ChatSessionDto
    {
        public Guid Id { get; set; }
        public Guid DeliveryId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public Guid AdminId { get; set; }
        public string AdminName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderType { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string MessageType { get; set; }
    }

    public class StartDeliveryChatSessionDto
    {
        public Guid DeliveryId { get; set; }
        public string CustomerId { get; set; } // Changed to string to accept phone number
        public string CustomerPhoneNumber { get; set; }
        public string AdminId { get; set; } // Changed to string to accept JWT token user ID
        public string AdminName { get; set; }
    }

    public class SendDeliveryChatMessageDto
    {
        public Guid SessionId { get; set; }
        public string SenderId { get; set; } // Changed to string to accept "admin" or Guid
        public string SenderType { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "text";
    }

    public class MarkMessagesAsReadDto
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
    }

    public class ChatSessionWithMessagesDto : ChatSessionDto
    {
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}
