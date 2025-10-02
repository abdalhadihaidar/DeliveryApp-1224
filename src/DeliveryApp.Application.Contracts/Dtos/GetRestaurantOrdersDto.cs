using System;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class GetRestaurantOrdersDto
    {
        public Guid RestaurantId { get; set; }
        public OrderStatus? Status { get; set; }
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
    }
}
