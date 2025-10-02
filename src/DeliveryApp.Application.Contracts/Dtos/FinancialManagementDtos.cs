using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// Financial dashboard request DTO
    /// </summary>
    public class FinancialDashboardRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? RestaurantId { get; set; }
        public string TimeZone { get; set; } = "UTC";
    }

    /// <summary>
    /// Financial dashboard response DTO
    /// </summary>
    public class FinancialDashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal GrowthRate { get; set; }
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
        public List<TopRestaurantDto> TopRestaurants { get; set; } = new();
        public PaymentMethodBreakdownDto PaymentMethodBreakdown { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Daily revenue DTO
    /// </summary>
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    /// <summary>
    /// Top restaurant DTO
    /// </summary>
    public class TopRestaurantDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal CommissionPaid { get; set; }
    }

    /// <summary>
    /// Payment method breakdown DTO
    /// </summary>
    public class PaymentMethodBreakdownDto
    {
        public decimal CreditCardAmount { get; set; }
        public decimal DebitCardAmount { get; set; }
        public decimal DigitalWalletAmount { get; set; }
        public int CreditCardCount { get; set; }
        public int DebitCardCount { get; set; }
        public int DigitalWalletCount { get; set; }
    }

    /// <summary>
    /// Revenue analytics request DTO
    /// </summary>
    public class RevenueAnalyticsRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string GroupBy { get; set; } = "day"; // day, week, month
        public Guid? RestaurantId { get; set; }
        public string? Category { get; set; }
        public string TimeZone { get; set; } = "UTC";
    }

    /// <summary>
    /// Revenue analytics response DTO
    /// </summary>
    public class RevenueAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal PreviousPeriodRevenue { get; set; }
        public decimal GrowthPercentage { get; set; }
        public List<RevenueDataPointDto> RevenueData { get; set; } = new();
        public List<CategoryRevenueDto> CategoryBreakdown { get; set; } = new();
        public RevenueMetricsDto Metrics { get; set; } = new();
    }

    /// <summary>
    /// Revenue data point DTO
    /// </summary>
    public class RevenueDataPointDto
    {
        public DateTime Period { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public decimal NetRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    /// <summary>
    /// Category revenue DTO
    /// </summary>
    public class CategoryRevenueDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Revenue metrics DTO
    /// </summary>
    public class RevenueMetricsDto
    {
        public decimal HighestDayRevenue { get; set; }
        public decimal LowestDayRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal MedianOrderValue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
    }

    /// <summary>
    /// Financial summary request DTO
    /// </summary>
    public class FinancialSummaryRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TimeZone { get; set; } = "UTC";
    }

    /// <summary>
    /// Restaurant financial summary DTO
    /// </summary>
    public class RestaurantFinancialSummaryDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal NetEarnings { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal PendingPayout { get; set; }
        public decimal TotalPaidOut { get; set; }
        public List<MonthlyEarningsDto> MonthlyEarnings { get; set; } = new();
        public PayoutScheduleDto PayoutSchedule { get; set; } = new();
    }

    /// <summary>
    /// Monthly earnings DTO
    /// </summary>
    public class MonthlyEarningsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public decimal NetEarnings { get; set; }
        public int OrderCount { get; set; }
    }

    /// <summary>
    /// Payout schedule DTO
    /// </summary>
    public class PayoutScheduleDto
    {
        public string Frequency { get; set; } = "weekly"; // daily, weekly, monthly
        public int DayOfWeek { get; set; } = 1; // Monday = 1
        public int DayOfMonth { get; set; } = 1;
        public DateTime NextPayoutDate { get; set; }
        public decimal NextPayoutAmount { get; set; }
    }

    /// <summary>
    /// Financial report request DTO
    /// </summary>
    public class FinancialReportRequestDto
    {
        [Required]
        public string ReportType { get; set; } = string.Empty; // revenue, commission, payout, tax
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public Guid? RestaurantId { get; set; }
        public string Format { get; set; } = "json"; // json, csv, excel
        public bool IncludeDetails { get; set; } = true;
        public string TimeZone { get; set; } = "UTC";
    }

    /// <summary>
    /// Financial report response DTO
    /// </summary>
    public class FinancialReportDto
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public FinancialSummaryDto Summary { get; set; } = new();
        public List<FinancialTransactionDetailDto> Details { get; set; } = new();
        public string? DownloadUrl { get; set; }
    }

    /// <summary>
    /// Financial summary DTO
    /// </summary>
    public class FinancialSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }
    }

    /// <summary>
    /// Financial transaction detail DTO
    /// </summary>
    public class FinancialTransactionDetailDto
    {
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RestaurantName { get; set; }
        public string? OrderReference { get; set; }
        public string? PaymentMethod { get; set; }
    }

    /// <summary>
    /// Payout processing request DTO
    /// </summary>
    public class PayoutProcessingRequestDto
    {
        [Required]
        public Guid RestaurantId { get; set; }
        
        public decimal? Amount { get; set; } // If null, process all pending
        public string? Description { get; set; }
        public bool ForceProcess { get; set; } = false;
    }

    /// <summary>
    /// Payout processing result DTO
    /// </summary>
    public class PayoutProcessingResultDto
    {
        public bool Success { get; set; }
        public string? PayoutId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int TransactionCount { get; set; }
        public string? StripeTransferId { get; set; }
    }

    /// <summary>
    /// Pending payout DTO
    /// </summary>
    public class PendingPayoutDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public decimal PendingAmount { get; set; }
        public int TransactionCount { get; set; }
        public DateTime OldestTransactionDate { get; set; }
        public DateTime NextScheduledPayout { get; set; }
        public bool IsEligibleForPayout { get; set; }
        public string? ConnectedAccountId { get; set; }
        public bool AccountSetupComplete { get; set; }
    }

    /// <summary>
    /// Transaction history request DTO
    /// </summary>
    public class TransactionHistoryRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? RestaurantId { get; set; }
        public string? TransactionType { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// Transaction history response DTO
    /// </summary>
    public class TransactionHistoryDto
    {
        public List<TransactionHistoryItemDto> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Transaction history item DTO
    /// </summary>
    public class TransactionHistoryItemDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RestaurantName { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderReference { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ExternalTransactionId { get; set; }
    }

    /// <summary>
    /// Commission calculation DTO
    /// </summary>
    public class CommissionCalculationDto
    {
        public Guid RestaurantId { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal RestaurantEarnings { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal NetAmount { get; set; }
        public string Currency { get; set; } = "SYP";
    }

    /// <summary>
    /// Platform metrics request DTO
    /// </summary>
    public class PlatformMetricsRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TimeZone { get; set; } = "UTC";
    }

    /// <summary>
    /// Platform financial metrics DTO
    /// </summary>
    public class PlatformFinancialMetricsDto
    {
        public decimal TotalGrossRevenue { get; set; }
        public decimal TotalCommissionRevenue { get; set; }
        public decimal TotalProcessingFees { get; set; }
        public decimal NetPlatformRevenue { get; set; }
        public int TotalRestaurants { get; set; }
        public int ActiveRestaurants { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal AverageCommissionRate { get; set; }
        public List<PlatformMetricDataPointDto> MetricsOverTime { get; set; } = new();
        public RestaurantPerformanceMetricsDto RestaurantMetrics { get; set; } = new();
    }

    /// <summary>
    /// Platform metric data point DTO
    /// </summary>
    public class PlatformMetricDataPointDto
    {
        public DateTime Date { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal CommissionRevenue { get; set; }
        public decimal NetRevenue { get; set; }
        public int OrderCount { get; set; }
        public int ActiveRestaurants { get; set; }
    }

    /// <summary>
    /// Financial export request DTO
    /// </summary>
    public class FinancialExportRequestDto
    {
        [Required]
        public string ExportType { get; set; } = string.Empty; // transactions, revenue, commissions
        
        [Required]
        public string Format { get; set; } = "csv"; // csv, excel
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public Guid? RestaurantId { get; set; }
        public List<string> Columns { get; set; } = new();
        public string TimeZone { get; set; } = "UTC";
    }

    /// <summary>
    /// File export DTO
    /// </summary>
    public class FileExportDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string DownloadUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Tax reporting request DTO
    /// </summary>
    public class TaxReportingRequestDto
    {
        [Required]
        public int Year { get; set; }
        
        public int? Quarter { get; set; }
        public Guid? RestaurantId { get; set; }
        public string ReportType { get; set; } = "annual"; // annual, quarterly, monthly
    }

    /// <summary>
    /// Tax reporting DTO
    /// </summary>
    public class TaxReportingDto
    {
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal EstimatedTaxes { get; set; }
        public List<TaxReportingDetailDto> Details { get; set; } = new();
        public string? Form1099Data { get; set; }
    }

    /// <summary>
    /// Tax reporting detail DTO
    /// </summary>
    public class TaxReportingDetailDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal CommissionsPaid { get; set; }
        public string TaxId { get; set; } = string.Empty;
        public bool Requires1099 { get; set; }
    }

    /// <summary>
    /// Payment reconciliation request DTO
    /// </summary>
    public class PaymentReconciliationRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string? BankStatementData { get; set; }
        public bool AutoReconcile { get; set; } = true;
    }

    /// <summary>
    /// Payment reconciliation DTO
    /// </summary>
    public class PaymentReconciliationDto
    {
        public int TotalTransactions { get; set; }
        public int ReconciledTransactions { get; set; }
        public int UnreconciledTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReconciledAmount { get; set; }
        public decimal UnreconciledAmount { get; set; }
        public List<UnreconciledTransactionDto> UnreconciledItems { get; set; } = new();
        public DateTime ReconciliationDate { get; set; }
    }

    /// <summary>
    /// Unreconciled transaction DTO
    /// </summary>
    public class UnreconciledTransactionDto
    {
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? ExternalTransactionId { get; set; }
    }
}

