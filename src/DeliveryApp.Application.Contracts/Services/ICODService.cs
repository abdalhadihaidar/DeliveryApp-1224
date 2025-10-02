using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface ICODService
    {
        /// <summary>
        /// Check if a delivery person has sufficient cash balance for an order
        /// </summary>
        Task<bool> HasSufficientCashBalanceAsync(Guid deliveryPersonId, decimal orderAmount);
        
        /// <summary>
        /// Get current cash balance for a delivery person
        /// </summary>
        Task<decimal> GetCashBalanceAsync(Guid deliveryPersonId);
        
        /// <summary>
        /// Update cash balance for a delivery person
        /// </summary>
        Task<bool> UpdateCashBalanceAsync(Guid deliveryPersonId, decimal amount, string reason);
        
        /// <summary>
        /// Create a COD transaction when driver pays restaurant
        /// </summary>
        Task<CODTransactionDto> CreateDriverToRestaurantTransactionAsync(Guid orderId, Guid deliveryPersonId, decimal amount);
        
        /// <summary>
        /// Create a COD transaction when customer pays driver
        /// </summary>
        Task<CODTransactionDto> CreateCustomerToDriverTransactionAsync(Guid orderId, Guid deliveryPersonId, decimal amount);
        
        /// <summary>
        /// Complete a COD transaction
        /// </summary>
        Task<bool> CompleteTransactionAsync(Guid transactionId, string notes = null);
        
        /// <summary>
        /// Cancel a COD transaction
        /// </summary>
        Task<bool> CancelTransactionAsync(Guid transactionId, string reason);
        
        /// <summary>
        /// Get COD transactions for a delivery person
        /// </summary>
        Task<List<CODTransactionDto>> GetTransactionsByDeliveryPersonAsync(Guid deliveryPersonId, int skipCount = 0, int maxResultCount = 10);
        
        /// <summary>
        /// Get COD transactions for an order
        /// </summary>
        Task<List<CODTransactionDto>> GetTransactionsByOrderAsync(Guid orderId);
        
        /// <summary>
        /// Process COD payment flow: driver pays restaurant, then collects from customer
        /// </summary>
        Task<CODPaymentResultDto> ProcessCODPaymentAsync(Guid orderId, Guid deliveryPersonId);
        
        /// <summary>
        /// Set delivery person's COD preferences
        /// </summary>
        Task<bool> SetCODPreferencesAsync(Guid deliveryPersonId, bool acceptsCOD, decimal maxCashLimit);
    }
}
