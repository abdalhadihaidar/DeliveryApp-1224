using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    /// <summary>
    /// Cash on Delivery (COD) management controller
    /// </summary>
    [ApiController]
    [Route("api/cod")]
    [Authorize]
    public class CODController : AbpController
    {
        private readonly ICODService _codService;

        public CODController(ICODService codService)
        {
            _codService = codService;
        }

        /// <summary>
        /// Check if delivery person has sufficient cash balance for an order
        /// </summary>
        [HttpGet("check-balance/{deliveryPersonId}")]
        public async Task<ActionResult<decimal>> CheckCashBalanceAsync(Guid deliveryPersonId, [FromQuery] decimal orderAmount)
        {
            var hasSufficientBalance = await _codService.HasSufficientCashBalanceAsync(deliveryPersonId, orderAmount);
            var currentBalance = await _codService.GetCashBalanceAsync(deliveryPersonId);
            
            return Ok(new { HasSufficientBalance = hasSufficientBalance, CurrentBalance = currentBalance });
        }

        /// <summary>
        /// Get current cash balance for a delivery person
        /// </summary>
        [HttpGet("balance/{deliveryPersonId}")]
        public async Task<ActionResult<decimal>> GetCashBalanceAsync(Guid deliveryPersonId)
        {
            var balance = await _codService.GetCashBalanceAsync(deliveryPersonId);
            return Ok(balance);
        }

        /// <summary>
        /// Update cash balance for a delivery person
        /// </summary>
        [HttpPost("update-balance")]
        public async Task<ActionResult<bool>> UpdateCashBalanceAsync([FromBody] UpdateCashBalanceRequestDto request)
        {
            var amount = request.IsAddition ? request.Amount : -request.Amount;
            var success = await _codService.UpdateCashBalanceAsync(request.DeliveryPersonId, amount, request.Reason);
            return Ok(success);
        }

        /// <summary>
        /// Process COD payment flow for an order
        /// </summary>
        [HttpPost("process-payment")]
        public async Task<ActionResult<CODPaymentResultDto>> ProcessCODPaymentAsync([FromBody] CODTransactionRequestDto request)
        {
            var result = await _codService.ProcessCODPaymentAsync(request.OrderId, request.DeliveryPersonId);
            return Ok(result);
        }

        /// <summary>
        /// Complete a COD transaction
        /// </summary>
        [HttpPost("complete-transaction/{transactionId}")]
        public async Task<ActionResult<bool>> CompleteTransactionAsync(Guid transactionId, [FromBody] string? notes = null)
        {
            var success = await _codService.CompleteTransactionAsync(transactionId, notes);
            return Ok(success);
        }

        /// <summary>
        /// Cancel a COD transaction
        /// </summary>
        [HttpPost("cancel-transaction/{transactionId}")]
        public async Task<ActionResult<bool>> CancelTransactionAsync(Guid transactionId, [FromBody] string reason)
        {
            var success = await _codService.CancelTransactionAsync(transactionId, reason);
            return Ok(success);
        }

        /// <summary>
        /// Get COD transactions for a delivery person
        /// </summary>
        [HttpGet("transactions/{deliveryPersonId}")]
        public async Task<ActionResult<List<CODTransactionDto>>> GetTransactionsByDeliveryPersonAsync(
            Guid deliveryPersonId, 
            [FromQuery] int skipCount = 0, 
            [FromQuery] int maxResultCount = 10)
        {
            var transactions = await _codService.GetTransactionsByDeliveryPersonAsync(deliveryPersonId, skipCount, maxResultCount);
            return Ok(transactions);
        }

        /// <summary>
        /// Get COD transactions for an order
        /// </summary>
        [HttpGet("transactions/order/{orderId}")]
        public async Task<ActionResult<List<CODTransactionDto>>> GetTransactionsByOrderAsync(Guid orderId)
        {
            var transactions = await _codService.GetTransactionsByOrderAsync(orderId);
            return Ok(transactions);
        }

        /// <summary>
        /// Set delivery person's COD preferences
        /// </summary>
        [HttpPost("preferences")]
        public async Task<ActionResult<bool>> SetCODPreferencesAsync([FromBody] CODPreferencesDto preferences)
        {
            var success = await _codService.SetCODPreferencesAsync(preferences.DeliveryPersonId, preferences.AcceptsCOD, preferences.MaxCashLimit);
            return Ok(success);
        }

        /// <summary>
        /// Get delivery person's COD preferences and balance
        /// </summary>
        [HttpGet("preferences/{deliveryPersonId}")]
        public async Task<ActionResult<CODPreferencesDto>> GetCODPreferencesAsync(Guid deliveryPersonId)
        {
            var balance = await _codService.GetCashBalanceAsync(deliveryPersonId);
            // Note: In a real implementation, you would get the preferences from the user entity
            // For now, returning a basic response
            var preferences = new CODPreferencesDto
            {
                DeliveryPersonId = deliveryPersonId,
                CurrentBalance = balance,
                AcceptsCOD = true, // Default value
                MaxCashLimit = 1000 // Default value
            };
            return Ok(preferences);
        }
    }
}
