using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Stripe payment service interface for handling payment processing
    /// </summary>
    [RemoteService]
    public interface IStripePaymentService : IApplicationService
    {
        /// <summary>
        /// Create a payment intent for processing payment
        /// </summary>
        /// <param name="request">Payment intent creation request</param>
        /// <returns>Payment intent response with client secret</returns>
        Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(CreatePaymentIntentRequestDto request);

        /// <summary>
        /// Confirm a payment intent
        /// </summary>
        /// <param name="paymentIntentId">Payment intent ID</param>
        /// <param name="paymentMethodId">Payment method ID</param>
        /// <returns>Payment confirmation result</returns>
        Task<ConfirmPaymentResponseDto> ConfirmPaymentAsync(string paymentIntentId, string paymentMethodId);

        /// <summary>
        /// Retrieve payment intent details
        /// </summary>
        /// <param name="paymentIntentId">Payment intent ID</param>
        /// <returns>Payment intent details</returns>
        Task<PaymentIntentDetailsDto> GetPaymentIntentAsync(string paymentIntentId);

        /// <summary>
        /// Process refund for a payment
        /// </summary>
        /// <param name="request">Refund request details</param>
        /// <returns>Refund processing result</returns>
        Task<RefundResponseDto> ProcessRefundAsync(ProcessRefundRequestDto request);

        /// <summary>
        /// Get list of payment methods for a customer
        /// </summary>
        /// <param name="customerId">Stripe customer ID</param>
        /// <returns>List of payment methods</returns>
        Task<List<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId);

        /// <summary>
        /// Create or retrieve Stripe customer
        /// </summary>
        /// <param name="request">Customer creation request</param>
        /// <returns>Stripe customer details</returns>
        Task<StripeCustomerDto> CreateOrGetCustomerAsync(CreateCustomerRequestDto request);

        /// <summary>
        /// Handle Stripe webhook events
        /// </summary>
        /// <param name="payload">Webhook payload</param>
        /// <param name="signature">Webhook signature</param>
        /// <returns>Webhook processing result</returns>
        Task<WebhookHandlingResultDto> HandleWebhookAsync(string payload, string signature);

        /// <summary>
        /// Calculate platform fees and commissions
        /// </summary>
        /// <param name="amount">Order amount</param>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <returns>Fee calculation result</returns>
        Task<FeeCalculationDto> CalculateFeesAsync(decimal amount, Guid restaurantId);

        /// <summary>
        /// Create connected account for restaurant
        /// </summary>
        /// <param name="request">Connected account creation request</param>
        /// <returns>Connected account details</returns>
        Task<ConnectedAccountDto> CreateConnectedAccountAsync(CreateConnectedAccountRequestDto request);

        /// <summary>
        /// Transfer funds to restaurant account
        /// </summary>
        /// <param name="request">Transfer request details</param>
        /// <returns>Transfer result</returns>
        Task<TransferResponseDto> TransferToRestaurantAsync(TransferRequestDto request);
    }
}

