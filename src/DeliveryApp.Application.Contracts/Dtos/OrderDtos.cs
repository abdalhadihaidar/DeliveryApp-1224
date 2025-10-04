using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CustomerName { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public Guid? DeliveryPersonId { get; set; }
        public string DeliveryPersonName { get; set; }
        public List<OrderItemDto> Items { get; set; }
        
        [Range(0, 10000)]
        public decimal Subtotal { get; set; }
        
        [Range(0, 1000)]
        public decimal DeliveryFee { get; set; }
        
        [Range(0, 1000)]
        public decimal Tax { get; set; }
        
        [Range(0, 10000)]
        public decimal TotalAmount { get; set; }
        
        public OrderStatus Status { get; set; }
        
        public PaymentStatus PaymentStatus { get; set; }
        
        public Guid DeliveryAddressId { get; set; }
        public AddressDto DeliveryAddress { get; set; }
        
        public Guid? PaymentMethodId { get; set; }
        public PaymentMethodDto PaymentMethod { get; set; }
        
        public DateTime OrderDate { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        
        public OrderDto()
        {
            Items = new List<OrderItemDto>();
        }
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid MenuItemId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Range(0, 1000)]
        public decimal Price { get; set; }
        
        [Range(0, 100)]
        public decimal Quantity { get; set; }
        
        public List<string> Options { get; set; }
        public List<string> SelectedOptions { get; set; }
        
        [StringLength(500)]
        public string SpecialInstructions { get; set; }
        
        public OrderItemDto()
        {
            Options = new List<string>();
            SelectedOptions = new List<string>();
        }
    }

    public class CreateOrderDto
    {
        [Required]
        public Guid RestaurantId { get; set; }
        
        [Required]
        public List<CreateOrderItemDto> Items { get; set; }
        
        [Required]
        public Guid DeliveryAddressId { get; set; }
        
        public Guid? PaymentMethodId { get; set; }
        
        [Range(0, 10000)]
        public decimal Subtotal { get; set; }
        
        [Range(0, 1000)]
        public decimal DeliveryFee { get; set; }
        
        [Range(0, 1000)]
        public decimal Tax { get; set; }
        
        [Range(0, 10000)]
        public decimal TotalAmount { get; set; }
        
        public bool? IsRushDelivery { get; set; } = false;
        public DateTime? PreferredDeliveryTime { get; set; }
        
        public CreateOrderDto()
        {
            Items = new List<CreateOrderItemDto>();
        }
    }

    public class CreateOrderItemDto
    {
        [Required]
        public Guid MenuItemId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Range(0, 1000)]
        public decimal Price { get; set; }
        
        [Range(0, 100)]
        public decimal Quantity { get; set; }
        
        public List<string> Options { get; set; }
        public List<string> SelectedOptions { get; set; }
        
        [StringLength(500)]
        public string SpecialInstructions { get; set; }
        
        public CreateOrderItemDto()
        {
            Options = new List<string>();
            SelectedOptions = new List<string>();
        }
    }

    // Admin DTOs
    public class GetOrderListDto
    {
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
        public string Sorting { get; set; } = "CreationTime desc";
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string RestaurantName { get; set; }
        public string CustomerName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SearchTerm { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelOrderDto
    {
        public string CancellationReason { get; set; }
        public decimal? RefundAmount { get; set; }
    }

    public class AssignDeliveryDto
    {
        public Guid DeliveryPersonId { get; set; }
    }

    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int ReadyOrders { get; set; }
        public int OutForDeliveryOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int OverdueOrders { get; set; }
        public double AverageDeliveryTime { get; set; }
    }
}
