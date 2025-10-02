using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Enums;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Services
{
    public interface IRestaurantOwnerAppService : IApplicationService
    {
        // Profile management
        Task<RestaurantOwnerDto> GetProfileAsync(string userId);
        Task<RestaurantOwnerDto> UpdateProfileAsync(UpdateUserDto input);
        
        // Restaurant management
        Task<List<RestaurantDto>> GetManagedRestaurantsAsync(string userId);
        Task<RestaurantDto> GetRestaurantDetailsAsync(Guid restaurantId, string userId);
        Task<RestaurantDto> CreateRestaurantAsync(CreateRestaurantDto input, string userId);
        Task<RestaurantDto> UpdateRestaurantAsync(Guid restaurantId, UpdateRestaurantDto input, string userId);
        
        // Menu management
        Task<List<MenuItemDto>> GetMenuItemsAsync(Guid restaurantId, string userId);
        Task<MenuItemDto> AddMenuItemAsync(Guid restaurantId, CreateMenuItemDto input, string userId);
        Task<MenuItemDto> UpdateMenuItemAsync(Guid restaurantId, Guid menuItemId, UpdateMenuItemDto input, string userId);
        Task<bool> DeleteMenuItemAsync(Guid restaurantId, Guid menuItemId, string userId);

        Task<bool> UpdateMenuItemAvailabilityAsync(Guid restaurantId, Guid menuItemId, bool isAvailable, string userId);
        Task<List<string>> GetRestaurantCategoriesAsync(Guid restaurantId, string userId);
        
        // Meal Category management
        Task<List<MealCategoryDto>> GetMealCategoriesAsync(Guid restaurantId, string userId);
        Task<MealCategoryDto> CreateMealCategoryAsync(Guid restaurantId, CreateMealCategoryDto input, string userId);
        Task<MealCategoryDto> UpdateMealCategoryAsync(Guid categoryId, UpdateMealCategoryDto input, string userId);
        Task<bool> DeleteMealCategoryAsync(Guid categoryId, string userId);
        
        // Offers management
        Task<List<SpecialOfferDto>> GetOffersAsync(Guid restaurantId, string userId);
        Task<SpecialOfferDto> CreateOfferAsync(Guid restaurantId, CreateSpecialOfferDto input, string userId);
        Task<SpecialOfferDto> UpdateOfferAsync(Guid offerId, UpdateSpecialOfferDto input, string userId);
        Task<bool> DeleteOfferAsync(Guid offerId, string userId);
        
        // Ad Requests management
        Task<List<AdRequestDto>> GetAdRequestsAsync(Guid restaurantId, string userId);
        Task<AdRequestDto> CreateAdRequestAsync(Guid restaurantId, CreateAdRequestDto input, string userId);
        Task<AdRequestDto> UpdateAdRequestAsync(Guid adRequestId, UpdateAdRequestDto input, string userId);
        Task<bool> DeleteAdRequestAsync(Guid adRequestId, string userId);
        
        // Order management
        Task<List<OrderDto>> GetRestaurantOrdersAsync(Guid restaurantId, OrderStatus? status, string userId);
        Task<OrderDto> GetOrderDetailsAsync(Guid orderId, string userId);
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, string userId);
        Task<bool> CancelOrderAsync(Guid orderId, string userId);

        // Monthly Reports
        Task<MonthlyRestaurantReportDto> GetMonthlyReportAsync(Guid restaurantId, int year, int month, string userId);
        Task<RestaurantPerformanceMetricsDto> GetPerformanceMetricsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, string userId);
        Task<List<PopularMenuItemDto>> GetTopSellingItemsAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, string userId);
        Task<CommissionSummaryDto> GetCommissionSummaryAsync(Guid restaurantId, DateTime fromDate, DateTime toDate, string userId);
        
        // Statistics
        Task<RestaurantStatisticsDto> GetRestaurantStatisticsAsync(Guid restaurantId, DateTime? startDate, DateTime? endDate, string userId);
        
        // Admin methods for restaurant approval
        Task<List<RestaurantDto>> GetPendingApprovalRestaurantsAsync();
        Task<RestaurantDto> ApproveRestaurantAsync(Guid restaurantId);
        Task<RestaurantDto> RejectRestaurantAsync(Guid restaurantId, string reason);
        
        // Restaurant owner method to check approval status
        Task<RestaurantApprovalStatusDto> GetRestaurantApprovalStatusAsync(Guid restaurantId);
        
        // Enhanced owner management methods
        Task<List<RestaurantOwnerDto>> GetAllRestaurantOwnersAsync(int skipCount = 0, int maxResultCount = 100);
        Task<RestaurantOwnerDto> GetOwnerDetailsAsync(Guid ownerId);
        Task<RestaurantOwnerStatisticsDto> GetOwnerStatisticsAsync(Guid ownerId, DateTime? startDate, DateTime? endDate);
        Task<List<RestaurantOwnerDto>> SearchOwnersAsync(string searchTerm, string status, DateTime? fromDate, DateTime? toDate, int skipCount = 0, int maxResultCount = 100);
        Task<bool> BulkApproveOwnersAsync(List<Guid> ownerIds);
        Task<bool> BulkRejectOwnersAsync(List<Guid> ownerIds, string reason);
        Task<bool> BulkActivateOwnersAsync(List<Guid> ownerIds);
        Task<bool> BulkDeactivateOwnersAsync(List<Guid> ownerIds);
        Task<RestaurantOwnerPerformanceDto> GetOwnerPerformanceAsync(Guid ownerId, DateTime fromDate, DateTime toDate);
        Task<List<RestaurantOwnerDto>> GetTopPerformingOwnersAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
        Task<RestaurantOwnerDashboardDto> GetOwnerDashboardAsync(Guid ownerId);
    }
}
