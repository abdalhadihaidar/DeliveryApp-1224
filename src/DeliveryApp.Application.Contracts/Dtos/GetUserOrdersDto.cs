using System;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class GetUserOrdersDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        public OrderStatus? Status { get; set; }
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
    }
}
