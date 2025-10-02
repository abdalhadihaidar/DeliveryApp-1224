using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Request DTO for creating a payment intent
    /// </summary>
    public class CreatePaymentIntentRequestDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "SYP";

        [Required]
        public Guid OrderId { get; set; }

        public string? CustomerId { get; set; }

        public string? PaymentMethodId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool AutomaticPaymentMethods { get; set; } = true;

        public bool CaptureMethod { get; set; } = true;

        public decimal? ApplicationFeeAmount { get; set; }

        public string? ConnectedAccountId { get; set; }
    }

    /// <summary>
    /// Response DTO for payment intent creation
    /// </summary>
    public class CreatePaymentIntentResponseDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool RequiresAction { get; set; }
        public string? NextActionType { get; set; }
    }

    /// <summary>
    /// Request DTO for confirming payment
    /// </summary>
    public class ConfirmPaymentRequestDto
    {
        [Required]
        public string PaymentIntentId { get; set; } = string.Empty;

        [Required]
        public string PaymentMethodId { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// Response DTO for payment confirmation
    /// </summary>
    public class ConfirmPaymentResponseDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public bool RequiresAction { get; set; }
        public string? NextActionType { get; set; }
        public string? ClientSecret { get; set; }
    }

    /// <summary>
    /// Payment intent details DTO
    /// </summary>
    public class PaymentIntentDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerId { get; set; }
        public string? PaymentMethodId { get; set; }
        public string? ReceiptEmail { get; set; }
        public decimal? ApplicationFeeAmount { get; set; }
        public PaymentChargeDto? LatestCharge { get; set; }
    }

    /// <summary>
    /// Payment charge details DTO
    /// </summary>
    public class PaymentChargeDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool Paid { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ReceiptUrl { get; set; }
        public PaymentMethodDetailsDto? PaymentMethodDetails { get; set; }
    }

    /// <summary>
    /// Payment method details DTO
    /// </summary>
    public class PaymentMethodDetailsDto
    {
        public string Type { get; set; } = string.Empty;
        public CardDetailsDto? Card { get; set; }
    }

    /// <summary>
    /// Card details DTO
    /// </summary>
    public class CardDetailsDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string? Country { get; set; }
        public string? Funding { get; set; }
    }

    /// <summary>
    /// Refund request DTO
    /// </summary>
    public class ProcessRefundRequestDto
    {
        [Required]
        public string PaymentIntentId { get; set; } = string.Empty;

        public decimal? Amount { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public bool ReverseTransfer { get; set; } = false;

        public decimal? RefundApplicationFee { get; set; }
    }

    /// <summary>
    /// Refund response DTO
    /// </summary>
    public class RefundResponseDto
    {
        public string RefundId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Stripe customer creation request DTO
    /// </summary>
    public class CreateCustomerRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Name { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public AddressDto? Address { get; set; }
    }

    /// <summary>
    /// Stripe customer DTO
    /// </summary>
    public class StripeCustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public AddressDto? Address { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Webhook handling result DTO
    /// </summary>
    public class WebhookHandlingResultDto
    {
        public bool Success { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Fee calculation DTO
    /// </summary>
    public class FeeCalculationDto
    {
        public decimal OrderAmount { get; set; }
        public decimal StripeFee { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal RestaurantCommission { get; set; }
        public decimal RestaurantPayout { get; set; }
        public decimal TotalFees { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    /// <summary>
    /// Connected account creation request DTO
    /// </summary>
    public class CreateConnectedAccountRequestDto
    {
        [Required]
        public Guid RestaurantId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string BusinessType { get; set; } = "individual";

        [Required]
        public string Country { get; set; } = "US";

        public BusinessDetailsDto? BusinessDetails { get; set; }
        public IndividualDetailsDto? IndividualDetails { get; set; }
    }

    /// <summary>
    /// Business details DTO
    /// </summary>
    public class BusinessDetailsDto
    {
        public string? Name { get; set; }
        public string? TaxId { get; set; }
        public AddressDto? Address { get; set; }
        public string? Phone { get; set; }
        public string? Url { get; set; }
    }

    /// <summary>
    /// Individual details DTO
    /// </summary>
    public class IndividualDetailsDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public AddressDto? Address { get; set; }
        public string? SsnLast4 { get; set; }
    }

    /// <summary>
    /// Connected account DTO
    /// </summary>
    public class ConnectedAccountDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool ChargesEnabled { get; set; }
        public bool PayoutsEnabled { get; set; }
        public bool DetailsSubmitted { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Requirements { get; set; } = new();
    }

    /// <summary>
    /// Transfer request DTO
    /// </summary>
    public class TransferRequestDto
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "SYP";

        [Required]
        public string DestinationAccountId { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? SourceTransaction { get; set; }
    }

    /// <summary>
    /// Transfer response DTO
    /// </summary>
    public class TransferResponseDto
    {
        public string TransferId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string DestinationAccountId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

