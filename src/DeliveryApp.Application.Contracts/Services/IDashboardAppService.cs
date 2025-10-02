
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IDashboardAppService : IApplicationService
    {
        Task<DashboardOverviewDto> GetDashboardOverviewAsync();
        Task<PagedResultDto<DashboardReviewDto>> GetReviewsAsync(int page, int pageSize, string sortBy, string sortOrder, string storeId, string customerId, int? minRating, int? maxRating);
        Task<PagedResultDto<CurrentDeliveryDto>> GetCurrentDeliveriesAsync(int page, int pageSize, string sortBy, string sortOrder);
        Task<PagedResultDto<DashboardCustomerDto>> GetCustomersAsync(int page, int pageSize, string sortBy, string sortOrder, string city, string interactionStatus);
        Task<PagedResultDto<CancelledOrderDto>> GetCancelledOrdersAsync(int page, int pageSize, string sortBy, string sortOrder);
        Task<PagedResultDto<CompletedOrderDto>> GetCompletedOrdersAsync(int page, int pageSize, string sortBy, string sortOrder);
        Task<PagedResultDto<TimeDifferenceDto>> GetTimeDifferenceAnalysisAsync(int page, int pageSize, string sortBy, string sortOrder);
        Task<PagedResultDto<StoreDto>> GetStoresAsync(int page, int pageSize, string sortBy, string sortOrder);
        Task<PreviousPeriodDataDto> GetPreviousPeriodDataAsync();
        Task<List<DashboardRecentActivityDto>> GetRecentActivitiesAsync();
    }
}


