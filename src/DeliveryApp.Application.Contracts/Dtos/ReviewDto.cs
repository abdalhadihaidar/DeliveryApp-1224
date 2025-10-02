using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserImageUrl { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime Date { get; set; }
    }
}
