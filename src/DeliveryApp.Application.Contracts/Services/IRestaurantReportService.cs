using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    /// <summary>
    /// Service for generating comprehensive restaurant reports including orders, commissions, and performance metrics
    /// </summary>
    public interface IRestaurantReportService : IApplicationService
    {
        /// <summary>
        /// Generate comprehensive monthly report for a restaurant
        /// </summary>
        /// <param name="request">Report request parameters</param>
        Task<MonthlyRestaurantReportDto> GenerateMonthlyReportAsync(ReportRequestDto request);

        /// <summary>
        /// Get restaurant performance metrics for a date range
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        Task<RestaurantPerformanceMetricsDto> GetPerformanceMetricsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Generate yearly summary report for a restaurant
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="year">Year for the report</param>
        Task<YearlyRestaurantSummaryDto> GenerateYearlySummaryAsync(Guid restaurantId, int year);

        /// <summary>
        /// Get custom report based on date range and type
        /// </summary>
        /// <param name="request">Custom report request</param>
        Task<CustomReportDto> GenerateCustomReportAsync(DateRangeReportRequestDto request);

        /// <summary>
        /// Get top selling menu items for a restaurant in a time period
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="topCount">Number of top items to return</param>
        Task<List<PopularMenuItemDto>> GetTopSellingItemsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, int topCount = 10);

        /// <summary>
        /// Get daily order breakdown for a restaurant in a date range
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        Task<List<DailyOrderSummaryDto>> GetDailyBreakdownAsync(Guid restaurantId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get hourly order volume analysis
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        Task<List<HourlyOrderVolumeDto>> GetHourlyOrderVolumeAsync(Guid restaurantId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get commission summary for a restaurant owner
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        Task<CommissionSummaryDto> GetCommissionSummaryAsync(Guid restaurantId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Export report data in various formats (PDF, Excel, CSV)
        /// </summary>
        /// <param name="reportData">Report data to export</param>
        /// <param name="format">Export format (pdf, excel, csv)</param>
        Task<byte[]> ExportReportAsync(object reportData, string format);

        /// <summary>
        /// Schedule automatic report generation and delivery
        /// </summary>
        /// <param name="restaurantId">Restaurant ID</param>
        /// <param name="frequency">Report frequency (daily, weekly, monthly)</param>
        /// <param name="email">Email to send reports to</param>
        Task<bool> ScheduleAutomaticReportAsync(Guid restaurantId, string frequency, string email);
    }

    /// <summary>
    /// Additional DTO for commission summary
    /// </summary>
    public class CommissionSummaryDto
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalOrderValue { get; set; }
        public decimal TotalCommissionDue { get; set; }
        public decimal CommissionRate { get; set; }
        public int TotalOrders { get; set; }

        public List<DailyCommissionDto> DailyBreakdown { get; set; } = new List<DailyCommissionDto>();
    }

    public class DailyCommissionDto
    {
        public DateTime Date { get; set; }
        public decimal OrderValue { get; set; }
        public decimal CommissionDue { get; set; }
        public int OrderCount { get; set; }
    }
}
