using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    /// <summary>
    /// Financial management API controller
    /// </summary>
    [ApiController]
    [Route("api/financial")]
    [Authorize]
    public class FinancialManagementController : AbpControllerBase
    {
        private readonly IFinancialManagementService _financialManagementService;

        public FinancialManagementController(IFinancialManagementService financialManagementService)
        {
            _financialManagementService = financialManagementService;
        }

        /// <summary>
        /// Get financial dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<FinancialDashboardDto> GetDashboard([FromQuery] FinancialDashboardRequestDto request)
        {
            return await _financialManagementService.GetFinancialDashboardAsync(request);
        }

        /// <summary>
        /// Get revenue analytics
        /// </summary>
        [HttpGet("analytics/revenue")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<RevenueAnalyticsDto> GetRevenueAnalytics([FromQuery] RevenueAnalyticsRequestDto request)
        {
            return await _financialManagementService.GetRevenueAnalyticsAsync(request);
        }

        /// <summary>
        /// Get restaurant financial summary
        /// </summary>
        [HttpGet("restaurant/{restaurantId}/summary")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<RestaurantFinancialSummaryDto> GetRestaurantSummary(Guid restaurantId, [FromQuery] FinancialSummaryRequestDto request)
        {
            return await _financialManagementService.GetRestaurantFinancialSummaryAsync(restaurantId, request);
        }

        /// <summary>
        /// Generate financial report
        /// </summary>
        [HttpPost("reports")]
        [Authorize(Roles = "Admin")]
        public async Task<FinancialReportDto> GenerateReport([FromBody] FinancialReportRequestDto request)
        {
            return await _financialManagementService.GenerateFinancialReportAsync(request);
        }

        /// <summary>
        /// Process restaurant payout
        /// </summary>
        [HttpPost("payouts/process")]
        [Authorize(Roles = "Admin")]
        public async Task<PayoutProcessingResultDto> ProcessPayout([FromBody] PayoutProcessingRequestDto request)
        {
            return await _financialManagementService.ProcessRestaurantPayoutAsync(request);
        }

        /// <summary>
        /// Get pending payouts
        /// </summary>
        [HttpGet("payouts/pending")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<List<PendingPayoutDto>> GetPendingPayouts([FromQuery] Guid? restaurantId = null)
        {
            return await _financialManagementService.GetPendingPayoutsAsync(restaurantId);
        }

        /// <summary>
        /// Get transaction history
        /// </summary>
        [HttpGet("transactions")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<TransactionHistoryDto> GetTransactionHistory([FromQuery] TransactionHistoryRequestDto request)
        {
            return await _financialManagementService.GetTransactionHistoryAsync(request);
        }

        /// <summary>
        /// Calculate commission
        /// </summary>
        [HttpGet("commission/calculate")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<CommissionCalculationDto> CalculateCommission([FromQuery] Guid restaurantId, [FromQuery] decimal amount)
        {
            return await _financialManagementService.CalculateCommissionAsync(restaurantId, amount);
        }

        /// <summary>
        /// Get platform metrics
        /// </summary>
        [HttpGet("platform/metrics")]
        [Authorize(Roles = "Admin")]
        public async Task<PlatformFinancialMetricsDto> GetPlatformMetrics([FromQuery] PlatformMetricsRequestDto request)
        {
            return await _financialManagementService.GetPlatformFinancialMetricsAsync(request);
        }

        /// <summary>
        /// Export financial data
        /// </summary>
        [HttpPost("export")]
        [Authorize(Roles = "Admin,RestaurantOwner")]
        public async Task<FileExportDto> ExportData([FromBody] FinancialExportRequestDto request)
        {
            return await _financialManagementService.ExportFinancialDataAsync(request);
        }

        /// <summary>
        /// Get tax reporting data
        /// </summary>
        [HttpGet("tax-reporting")]
        [Authorize(Roles = "Admin")]
        public async Task<TaxReportingDto> GetTaxReporting([FromQuery] TaxReportingRequestDto request)
        {
            return await _financialManagementService.GetTaxReportingDataAsync(request);
        }

        /// <summary>
        /// Reconcile payments
        /// </summary>
        [HttpPost("reconciliation")]
        [Authorize(Roles = "Admin")]
        public async Task<PaymentReconciliationDto> ReconcilePayments([FromBody] PaymentReconciliationRequestDto request)
        {
            return await _financialManagementService.ReconcilePaymentsAsync(request);
        }
    }
}

