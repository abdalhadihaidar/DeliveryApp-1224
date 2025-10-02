using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace DeliveryApp.Domain.Entities
{
    public class Review : Entity<Guid>
    {
        public Guid RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;
        
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public Review()
        {
        }

        public Review(Guid id) : base(id)
        {
        }
    }
}
