using System;
using Volo.Abp.Domain.Entities;

namespace DeliveryApp.Domain.Entities
{
    public class ChatMessage : Entity<Guid>
    {
        public Guid SessionId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderType { get; set; } // "admin", "customer", "system"
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string MessageType { get; set; } = "text"; // "text", "system", "notification"

        protected ChatMessage()
        {
        }

        public ChatMessage(Guid id, Guid sessionId, Guid senderId, string senderType, string content, string messageType = "text")
            : base(id)
        {
            SessionId = sessionId;
            SenderId = senderId;
            SenderType = senderType;
            Content = content;
            MessageType = messageType;
            SentAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}
