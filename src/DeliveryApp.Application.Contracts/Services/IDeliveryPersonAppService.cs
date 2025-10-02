using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Enums;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Services
{
    [RemoteService]
    public interface IDeliveryPersonAppService : IApplicationService
    {
        // Profile management
        Task<DeliveryPersonDto> GetProfileAsync();
        Task<DeliveryPersonDto> UpdateProfileAsync(UpdateUserDto input);
        
        // Availability management
        Task<bool> SetAvailabilityAsync(bool isAvailable);
        Task<bool> UpdateLocationAsync(double latitude, double longitude);
        
        // Order management
        Task<List<OrderDto>> GetAvailableOrdersAsync();
        Task<List<OrderDto>> GetAssignedOrdersAsync();
        Task<OrderDto> AcceptOrderAsync(Guid orderId);
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
        Task<OrderDto> CompleteOrderAsync(Guid orderId);
        
        // Statistics
        Task<DeliveryStatisticsDto> GetDeliveryStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        // COD-related methods
        Task<decimal> GetCashBalanceAsync();
        Task<bool> UpdateCashBalanceAsync(decimal amount, string reason);
        Task<List<CODTransactionDto>> GetCODTransactionsAsync(int skipCount = 0, int maxResultCount = 10);
        Task<CODPaymentResultDto> ProcessCODPaymentAsync(Guid orderId);
        Task<bool> SetCODPreferencesAsync(bool acceptsCOD, decimal maxCashLimit);
        Task<CODPreferencesDto> GetCODPreferencesAsync();
        Task<bool> HasSufficientCashForOrderAsync(Guid orderId);
    }
}
