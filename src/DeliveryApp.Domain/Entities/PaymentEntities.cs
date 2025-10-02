using System;
using System.ComponentModel.DataAnnotations;
using DeliveryApp.Domain.Enums;
using Volo.Abp.Domain.Entities;

namespace DeliveryApp.Domain.Entities
{
    /// <summary>
    /// Payment transaction entity for tracking all payment operations
    /// </summary>
    public class PaymentTransaction : Entity<int>
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [StringLength(255)]
        public string PaymentIntentId { get; set; } = string.Empty;

        [StringLength(255)]
        public string? PaymentMethodId { get; set; }

        [StringLength(255)]
        public string? CustomerId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SYP";

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required]
        [StringLength(50)]
        public string PaymentProvider { get; set; } = string.Empty;

        public decimal PlatformFee { get; set; }

        public decimal RestaurantCommission { get; set; }

        [StringLength(255)]
        public string? RefundId { get; set; }

        public decimal? RefundAmount { get; set; }

        [StringLength(50)]
        public string? RefundStatus { get; set; }

        [StringLength(255)]
        public string? ConnectedAccountId { get; set; }

        [StringLength(255)]
        public string? TransferId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
    }

    /// <summary>
    /// Stripe customer entity for storing customer information
    /// </summary>
    public class StripeCustomer : Entity<int>
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string StripeCustomerId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Name { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual AppUser User { get; set; } = null!;
    }

    /// <summary>
    /// Connected account entity for restaurant payment accounts
    /// </summary>
    public class ConnectedAccount : Entity<int>
    {
        [Required]
        public Guid RestaurantId { get; set; }

        [Required]
        [StringLength(255)]
        public string StripeAccountId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BusinessType { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string Country { get; set; } = string.Empty;

        public bool ChargesEnabled { get; set; }

        public bool PayoutsEnabled { get; set; }

        public bool DetailsSubmitted { get; set; }

        [StringLength(1000)]
        public string? Requirements { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Restaurant Restaurant { get; set; } = null!;
    }

    /// <summary>
    /// Financial transaction entity for tracking all money movements
    /// </summary>
    public class FinancialTransaction : Entity<int>
    {
        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = string.Empty; // payment, refund, transfer, fee

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty; // order, restaurant, platform

        [Required]
        public int EntityId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SYP";

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [StringLength(255)]
        public string? ExternalTransactionId { get; set; }

        [StringLength(255)]
        public string? PaymentProvider { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [StringLength(1000)]
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Restaurant payout entity for tracking restaurant payments
    /// </summary>
    public class RestaurantPayout : Entity<int>
    {
        [Required]
        public Guid RestaurantId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SYP";

        public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

        [StringLength(255)]
        public string? StripeTransferId { get; set; }

        public DateTime PayoutDate { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public int OrderCount { get; set; }

        public decimal TotalOrderAmount { get; set; }

        public decimal PlatformFees { get; set; }

        public decimal StripeFees { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Restaurant Restaurant { get; set; } = null!;
    }
}

