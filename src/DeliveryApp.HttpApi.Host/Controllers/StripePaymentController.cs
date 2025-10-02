using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    /// <summary>
    /// Stripe payment API controller
    /// </summary>
    [ApiController]
    [Route("api/payments/stripe")]
    [Authorize]
    public class StripePaymentController : AbpControllerBase
    {
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;

        public StripePaymentController(IStripePaymentService stripePaymentService, IStringLocalizer<DeliveryAppResource> localizer)
        {
            _stripePaymentService = stripePaymentService;
            _localizer = localizer;
        }

        /// <summary>
        /// Create a payment intent for order payment
        /// </summary>
        /// <param name="request">Payment intent creation request</param>
        /// <returns>Payment intent with client secret</returns>
        [HttpPost("payment-intent")]
        public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntent([FromBody] CreatePaymentIntentRequestDto request)
        {
            return await _stripePaymentService.CreatePaymentIntentAsync(request);
        }

        /// <summary>
        /// Confirm a payment intent
        /// </summary>
        /// <param name="request">Payment confirmation request</param>
        /// <returns>Payment confirmation result</returns>
        [HttpPost("confirm-payment")]
        public async Task<ConfirmPaymentResponseDto> ConfirmPayment([FromBody] ConfirmPaymentRequestDto request)
        {
            return await _stripePaymentService.ConfirmPaymentAsync(request.PaymentIntentId, request.PaymentMethodId);
        }

        /// <summary>
        /// Get payment intent details
        /// </summary>
        /// <param name="paymentIntentId">Payment intent ID</param>
        /// <returns>Payment intent details</returns>
        [HttpGet("payment-intent/{paymentIntentId}")]
        public async Task<PaymentIntentDetailsDto> GetPaymentIntent(string paymentIntentId)
        {
            return await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId);
        }

        /// <summary>
        /// Process a refund
        /// </summary>
        /// <param name="request">Refund request</param>
        /// <returns>Refund processing result</returns>
        [HttpPost("refund")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<RefundResponseDto> ProcessRefund([FromBody] ProcessRefundRequestDto request)
        {
            return await _stripePaymentService.ProcessRefundAsync(request);
        }

        /// <summary>
        /// Get customer payment methods
        /// </summary>
        /// <param name="customerId">Stripe customer ID</param>
        /// <returns>List of payment methods</returns>
        [HttpGet("customer/{customerId}/payment-methods")]
        public async Task<List<PaymentMethodDto>> GetCustomerPaymentMethods(string customerId)
        {
            return await _stripePaymentService.GetCustomerPaymentMethodsAsync(customerId);
        }

        /// <summary>
        /// Create or get Stripe customer
        /// </summary>
        /// <param name="request">Customer creation request</param>
        /// <returns>Stripe customer details</returns>
        [HttpPost("customer")]
        public async Task<StripeCustomerDto> CreateOrGetCustomer([FromBody] CreateCustomerRequestDto request)
        {
            return await _stripePaymentService.CreateOrGetCustomerAsync(request);
        }

        /// <summary>
        /// Calculate fees for an order
        /// </summary>
        /// <param name="amount">Order amount</param>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <returns>Fee calculation</returns>
        [HttpGet("calculate-fees")]
        public async Task<FeeCalculationDto> CalculateFees([FromQuery] decimal amount, [FromQuery] Guid restaurantId)
        {
            return await _stripePaymentService.CalculateFeesAsync(amount, restaurantId);
        }

        /// <summary>
        /// Create connected account for restaurant
        /// </summary>
        /// <param name="request">Connected account creation request</param>
        /// <returns>Connected account details</returns>
        [HttpPost("connected-account")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<ConnectedAccountDto> CreateConnectedAccount([FromBody] CreateConnectedAccountRequestDto request)
        {
            return await _stripePaymentService.CreateConnectedAccountAsync(request);
        }

        /// <summary>
        /// Transfer funds to restaurant
        /// </summary>
        /// <param name="request">Transfer request</param>
        /// <returns>Transfer result</returns>
        [HttpPost("transfer")]
        [Authorize(Roles = "Admin")]
        public async Task<TransferResponseDto> TransferToRestaurant([FromBody] TransferRequestDto request)
        {
            return await _stripePaymentService.TransferToRestaurantAsync(request);
        }

        /// <summary>
        /// Handle Stripe webhooks
        /// </summary>
        /// <returns>Webhook handling result</returns>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            var payload = await Request.GetRawBodyStringAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            try
            {
                var result = await _stripePaymentService.HandleWebhookAsync(payload, signature);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing Stripe webhook");
                return StatusCode(500, new { error = _localizer["Payment:WebhookProcessingFailed"] });
            }
        }
    }

    /// <summary>
    /// Extension methods for request handling
    /// </summary>
    public static class HttpRequestExtensions
    {
        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            return await reader.ReadToEndAsync();
        }
    }
}

