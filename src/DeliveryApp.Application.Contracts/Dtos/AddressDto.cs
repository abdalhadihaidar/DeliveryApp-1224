using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? Title { get; set; }
        
        [Required]
        [StringLength(200)]
        public string? Street { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? City { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? State { get; set; }
        
        [Required]
        [StringLength(20)]
        public string? ZipCode { get; set; }
        
        [StringLength(500)]
        public string? FullAddress { get; set; }
        
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }
        
        public bool IsDefault { get; set; }
    }
}
