using System;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Domain.Entities;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class PaymentMethodDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public PaymentType? Type { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? Title { get; set; }
        
        [StringLength(4)]
        public string? LastFourDigits { get; set; }
        
        [StringLength(100)]
        public string? CardHolderName { get; set; }
        
        [StringLength(10)]
        public string? ExpiryDate { get; set; }
        
        public bool IsDefault { get; set; }
    }
}
