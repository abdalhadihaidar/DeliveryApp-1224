using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Cash on Delivery service for managing COD transactions and driver cash balances
    /// </summary>
    public class CODService : ApplicationService, ICODService, ITransientDependency
    {
        private readonly ICODTransactionRepository _codTransactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly ILogger<CODService> _logger;

        public CODService(
            ICODTransactionRepository codTransactionRepository,
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IRestaurantRepository restaurantRepository,
            ILogger<CODService> logger)
        {
            _codTransactionRepository = codTransactionRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _restaurantRepository = restaurantRepository;
            _logger = logger;
        }

        public async Task<bool> HasSufficientCashBalanceAsync(Guid deliveryPersonId, decimal orderAmount)
        {
            try
            {
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                if (deliveryPerson?.DeliveryStatus == null)
                {
                    return false;
                }

                var currentBalance = deliveryPerson.DeliveryStatus.CashBalance;
                var maxLimit = deliveryPerson.DeliveryStatus.MaxCashLimit;
                
                // Check if driver has enough cash and won't exceed limit after paying restaurant
                return currentBalance >= orderAmount && (currentBalance - orderAmount) <= maxLimit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cash balance for delivery person {DeliveryPersonId}", deliveryPersonId);
                return false;
            }
        }

        public async Task<decimal> GetCashBalanceAsync(Guid deliveryPersonId)
        {
            try
            {
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                return deliveryPerson?.DeliveryStatus?.CashBalance ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cash balance for delivery person {DeliveryPersonId}", deliveryPersonId);
                return 0;
            }
        }

        public async Task<bool> UpdateCashBalanceAsync(Guid deliveryPersonId, decimal amount, string reason)
        {
            try
            {
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                if (deliveryPerson?.DeliveryStatus == null)
                {
                    return false;
                }

                var newBalance = deliveryPerson.DeliveryStatus.CashBalance + amount;
                
                // Check if new balance exceeds maximum limit
                if (newBalance > deliveryPerson.DeliveryStatus.MaxCashLimit)
                {
                    _logger.LogWarning("Cash balance update rejected for delivery person {DeliveryPersonId}: would exceed max limit", deliveryPersonId);
                    return false;
                }

                deliveryPerson.DeliveryStatus.CashBalance = newBalance;
                await _userRepository.UpdateAsync(deliveryPerson);

                _logger.LogInformation("Updated cash balance for delivery person {DeliveryPersonId}: {Amount} ({Reason}). New balance: {NewBalance}", 
                    deliveryPersonId, amount, reason, newBalance);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cash balance for delivery person {DeliveryPersonId}", deliveryPersonId);
                return false;
            }
        }

        public async Task<CODTransactionDto> CreateDriverToRestaurantTransactionAsync(Guid orderId, Guid deliveryPersonId, decimal amount)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    throw new InvalidOperationException("Order not found");
                }

                var transaction = new CODTransaction
                {
                    OrderId = orderId,
                    DeliveryPersonId = deliveryPersonId,
                    RestaurantId = order.RestaurantId,
                    Amount = amount,
                    Type = CODTransactionType.DriverToRestaurant,
                    Status = CODTransactionStatus.Pending,
                    DriverPaidToRestaurant = amount,
                    Notes = $"Driver payment to restaurant for order {orderId}"
                };

                await _codTransactionRepository.InsertAsync(transaction);

                _logger.LogInformation("Created driver-to-restaurant COD transaction {TransactionId} for order {OrderId}", 
                    transaction.Id, orderId);

                return ObjectMapper.Map<CODTransaction, CODTransactionDto>(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver-to-restaurant transaction for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<CODTransactionDto> CreateCustomerToDriverTransactionAsync(Guid orderId, Guid deliveryPersonId, decimal amount)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    throw new InvalidOperationException("Order not found");
                }

                var transaction = new CODTransaction
                {
                    OrderId = orderId,
                    DeliveryPersonId = deliveryPersonId,
                    RestaurantId = order.RestaurantId,
                    Amount = amount,
                    Type = CODTransactionType.CustomerToDriver,
                    Status = CODTransactionStatus.Pending,
                    DriverCollectedFromCustomer = amount,
                    DriverProfit = order.DeliveryFee, // Driver keeps the delivery fee
                    Notes = $"Customer payment to driver for order {orderId}"
                };

                await _codTransactionRepository.InsertAsync(transaction);

                _logger.LogInformation("Created customer-to-driver COD transaction {TransactionId} for order {OrderId}", 
                    transaction.Id, orderId);

                return ObjectMapper.Map<CODTransaction, CODTransactionDto>(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer-to-driver transaction for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> CompleteTransactionAsync(Guid transactionId, string notes = null)
        {
            try
            {
                var transaction = await _codTransactionRepository.GetAsync(transactionId);
                transaction.Status = CODTransactionStatus.Completed;
                transaction.CompletedAt = DateTime.UtcNow;
                
                if (!string.IsNullOrEmpty(notes))
                {
                    transaction.Notes = notes;
                }

                await _codTransactionRepository.UpdateAsync(transaction);

                _logger.LogInformation("Completed COD transaction {TransactionId}", transactionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing COD transaction {TransactionId}", transactionId);
                return false;
            }
        }

        public async Task<bool> CancelTransactionAsync(Guid transactionId, string reason)
        {
            try
            {
                var transaction = await _codTransactionRepository.GetAsync(transactionId);
                transaction.Status = CODTransactionStatus.Cancelled;
                transaction.Notes = $"Cancelled: {reason}";

                await _codTransactionRepository.UpdateAsync(transaction);

                _logger.LogInformation("Cancelled COD transaction {TransactionId}: {Reason}", transactionId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling COD transaction {TransactionId}", transactionId);
                return false;
            }
        }

        public async Task<List<CODTransactionDto>> GetTransactionsByDeliveryPersonAsync(Guid deliveryPersonId, int skipCount = 0, int maxResultCount = 10)
        {
            try
            {
                var transactions = await _codTransactionRepository.GetTransactionsByDeliveryPersonAsync(deliveryPersonId, skipCount, maxResultCount, "CreatedAt DESC");
                return ObjectMapper.Map<List<CODTransaction>, List<CODTransactionDto>>(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for delivery person {DeliveryPersonId}", deliveryPersonId);
                return new List<CODTransactionDto>();
            }
        }

        public async Task<List<CODTransactionDto>> GetTransactionsByOrderAsync(Guid orderId)
        {
            try
            {
                var transactions = await _codTransactionRepository.GetTransactionsByOrderAsync(orderId);
                return ObjectMapper.Map<List<CODTransaction>, List<CODTransactionDto>>(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for order {OrderId}", orderId);
                return new List<CODTransactionDto>();
            }
        }

        public async Task<CODPaymentResultDto> ProcessCODPaymentAsync(Guid orderId, Guid deliveryPersonId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    return new CODPaymentResultDto
                    {
                        Success = false,
                        Message = "Order not found",
                        ErrorCode = "ORDER_NOT_FOUND"
                    };
                }

                // Check if driver has sufficient cash balance
                var hasSufficientBalance = await HasSufficientCashBalanceAsync(deliveryPersonId, order.TotalAmount);
                if (!hasSufficientBalance)
                {
                    return new CODPaymentResultDto
                    {
                        Success = false,
                        Message = "Insufficient cash balance for COD payment",
                        ErrorCode = "INSUFFICIENT_CASH_BALANCE"
                    };
                }

                // Step 1: Driver pays restaurant
                var driverToRestaurantTransaction = await CreateDriverToRestaurantTransactionAsync(orderId, deliveryPersonId, order.TotalAmount);
                
                // Update driver's cash balance (subtract amount paid to restaurant)
                await UpdateCashBalanceAsync(deliveryPersonId, -order.TotalAmount, $"Payment to restaurant for order {orderId}");

                // Step 2: Customer pays driver
                var customerToDriverTransaction = await CreateCustomerToDriverTransactionAsync(orderId, deliveryPersonId, order.TotalAmount);
                
                // Update driver's cash balance (add amount collected from customer + delivery fee)
                var totalCollected = order.TotalAmount + order.DeliveryFee;
                await UpdateCashBalanceAsync(deliveryPersonId, totalCollected, $"Collection from customer for order {orderId}");

                // Complete both transactions
                await CompleteTransactionAsync(driverToRestaurantTransaction.Id, "Driver payment to restaurant completed");
                await CompleteTransactionAsync(customerToDriverTransaction.Id, "Customer payment to driver completed");

                // Update order payment status
                order.PaymentStatus = PaymentStatus.Paid;
                await _orderRepository.UpdateAsync(order);

                var currentBalance = await GetCashBalanceAsync(deliveryPersonId);

                _logger.LogInformation("Successfully processed COD payment for order {OrderId} with delivery person {DeliveryPersonId}", 
                    orderId, deliveryPersonId);

                return new CODPaymentResultDto
                {
                    Success = true,
                    Message = "COD payment processed successfully",
                    DriverToRestaurantTransactionId = driverToRestaurantTransaction.Id,
                    CustomerToDriverTransactionId = customerToDriverTransaction.Id,
                    DriverCashBalanceAfter = currentBalance,
                    DriverProfit = order.DeliveryFee
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing COD payment for order {OrderId}", orderId);
                return new CODPaymentResultDto
                {
                    Success = false,
                    Message = "Error processing COD payment",
                    ErrorCode = "PROCESSING_ERROR"
                };
            }
        }

        public async Task<bool> SetCODPreferencesAsync(Guid deliveryPersonId, bool acceptsCOD, decimal maxCashLimit)
        {
            try
            {
                var deliveryPerson = await _userRepository.GetAsync(deliveryPersonId);
                if (deliveryPerson?.DeliveryStatus == null)
                {
                    return false;
                }

                deliveryPerson.DeliveryStatus.AcceptsCOD = acceptsCOD;
                deliveryPerson.DeliveryStatus.MaxCashLimit = maxCashLimit;

                await _userRepository.UpdateAsync(deliveryPerson);

                _logger.LogInformation("Updated COD preferences for delivery person {DeliveryPersonId}: AcceptsCOD={AcceptsCOD}, MaxLimit={MaxLimit}", 
                    deliveryPersonId, acceptsCOD, maxCashLimit);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting COD preferences for delivery person {DeliveryPersonId}", deliveryPersonId);
                return false;
            }
        }
    }
}
