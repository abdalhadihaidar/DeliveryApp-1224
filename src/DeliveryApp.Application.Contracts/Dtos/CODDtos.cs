using System;
using System.Collections.Generic;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Application.Dtos;

namespace DeliveryApp.Application.Contracts.Dtos
{
    public class CODTransactionDto : EntityDto<Guid>
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryPersonId { get; set; }
        public string DeliveryPersonName { get; set; } = string.Empty;
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public CODTransactionType Type { get; set; }
        public CODTransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public decimal DriverPaidToRestaurant { get; set; }
        public decimal DriverCollectedFromCustomer { get; set; }
        public decimal DriverProfit { get; set; }
    }

    public class CODPaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public Guid? DriverToRestaurantTransactionId { get; set; }
        public Guid? CustomerToDriverTransactionId { get; set; }
        public decimal DriverCashBalanceAfter { get; set; }
        public decimal DriverProfit { get; set; }
    }

    public class DeliveryPersonCashBalanceDto
    {
        public Guid DeliveryPersonId { get; set; }
        public string DeliveryPersonName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public decimal MaxCashLimit { get; set; }
        public bool AcceptsCOD { get; set; }
        public decimal AvailableBalance { get; set; }
        public List<CODTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class CODPreferencesDto
    {
        public Guid DeliveryPersonId { get; set; }
        public bool AcceptsCOD { get; set; }
        public decimal MaxCashLimit { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class CODTransactionRequestDto
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryPersonId { get; set; }
        public decimal Amount { get; set; }
        public CODTransactionType Type { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateCashBalanceRequestDto
    {
        public Guid DeliveryPersonId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsAddition { get; set; } = true; // true for addition, false for subtraction
    }
}
