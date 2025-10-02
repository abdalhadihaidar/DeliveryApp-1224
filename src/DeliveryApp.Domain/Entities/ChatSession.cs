using System;
using Volo.Abp.Domain.Entities;

namespace DeliveryApp.Domain.Entities
{
    public class ChatSession : Entity<Guid>
    {
        public Guid DeliveryId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public Guid AdminId { get; set; }
        public string AdminName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; }

        protected ChatSession()
        {
        }

        public ChatSession(Guid id, Guid deliveryId, Guid customerId, string customerPhoneNumber, Guid adminId, string adminName)
            : base(id)
        {
            DeliveryId = deliveryId;
            CustomerId = customerId;
            CustomerPhoneNumber = customerPhoneNumber;
            AdminId = adminId;
            AdminName = adminName;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
