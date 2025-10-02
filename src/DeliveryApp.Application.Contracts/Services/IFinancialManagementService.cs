using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Financial management service interface for comprehensive financial operations
    /// </summary>
    [RemoteService]
    public interface IFinancialManagementService : IApplicationService
    {
        /// <summary>
        /// Get financial dashboard data
        /// </summary>
        /// <param name="request">Dashboard request parameters</param>
        /// <returns>Financial dashboard data</returns>
        Task<FinancialDashboardDto> GetFinancialDashboardAsync(FinancialDashboardRequestDto request);

        /// <summary>
        /// Get revenue analytics for a specific period
        /// </summary>
        /// <param name="request">Revenue analytics request</param>
        /// <returns>Revenue analytics data</returns>
        Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(RevenueAnalyticsRequestDto request);

        /// <summary>
        /// Get restaurant financial summary
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="request">Financial summary request</param>
        /// <returns>Restaurant financial summary</returns>
        Task<RestaurantFinancialSummaryDto> GetRestaurantFinancialSummaryAsync(Guid restaurantId, FinancialSummaryRequestDto request);

        /// <summary>
        /// Generate financial report
        /// </summary>
        /// <param name="request">Financial report request</param>
        /// <returns>Financial report data</returns>
        Task<FinancialReportDto> GenerateFinancialReportAsync(FinancialReportRequestDto request);

        /// <summary>
        /// Process restaurant payout
        /// </summary>
        /// <param name="request">Payout processing request</param>
        /// <returns>Payout processing result</returns>
        Task<PayoutProcessingResultDto> ProcessRestaurantPayoutAsync(PayoutProcessingRequestDto request);

        /// <summary>
        /// Get pending payouts for restaurants
        /// </summary>
        /// <param name="restaurantId">Optional restaurant ID filter</param>
        /// <returns>List of pending payouts</returns>
        Task<List<PendingPayoutDto>> GetPendingPayoutsAsync(Guid? restaurantId = null);

        /// <summary>
        /// Get transaction history
        /// </summary>
        /// <param name="request">Transaction history request</param>
        /// <returns>Transaction history data</returns>
        Task<TransactionHistoryDto> GetTransactionHistoryAsync(TransactionHistoryRequestDto request);

        /// <summary>
        /// Calculate commission for a restaurant
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="amount">Order amount</param>
        /// <returns>Commission calculation</returns>
        Task<CommissionCalculationDto> CalculateCommissionAsync(Guid restaurantId, decimal amount);

        /// <summary>
        /// Get platform financial metrics
        /// </summary>
        /// <param name="request">Platform metrics request</param>
        /// <returns>Platform financial metrics</returns>
        Task<PlatformFinancialMetricsDto> GetPlatformFinancialMetricsAsync(PlatformMetricsRequestDto request);

        /// <summary>
        /// Export financial data to CSV/Excel
        /// </summary>
        /// <param name="request">Export request</param>
        /// <returns>Export file data</returns>
        Task<FileExportDto> ExportFinancialDataAsync(FinancialExportRequestDto request);

        /// <summary>
        /// Get tax reporting data
        /// </summary>
        /// <param name="request">Tax reporting request</param>
        /// <returns>Tax reporting data</returns>
        Task<TaxReportingDto> GetTaxReportingDataAsync(TaxReportingRequestDto request);

        /// <summary>
        /// Reconcile payments with bank statements
        /// </summary>
        /// <param name="request">Reconciliation request</param>
        /// <returns>Reconciliation result</returns>
        Task<PaymentReconciliationDto> ReconcilePaymentsAsync(PaymentReconciliationRequestDto request);
    }
}

