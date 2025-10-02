using System;
using System.Collections.Generic;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public partial class ScheduledNotification
    {
        // Convenience properties expected by the application layer
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Custom;
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> RecipientIds { get; set; } = new();
        public DateTime ScheduledAt { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public string ImageUrl { get; set; }
        public string ActionUrl { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 
