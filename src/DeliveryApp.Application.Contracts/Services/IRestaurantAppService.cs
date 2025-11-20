using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Services
{
    [RemoteService]
    public interface IRestaurantAppService : IApplicationService
    {
        Task<List<RestaurantDto>> GetListAsync(GetRestaurantListDto input);
        Task<RestaurantDto> GetAsync(Guid id);
        Task<List<RestaurantDto>> GetByIdsAsync(List<Guid> ids);
        Task<List<RestaurantDto>> SearchAsync(string searchTerm, int maxResultCount = 10);
        Task<List<MenuItemDto>> SearchMealsAsync(string searchTerm, int maxResultCount = 10);
        Task<List<RestaurantDto>> GetByCategoryAsync(string category, int skipCount = 0, int maxResultCount = 10);
        Task<List<ReviewDto>> GetRestaurantReviewsAsync(Guid restaurantId);
        Task<ReviewDto> AddReviewAsync(Guid restaurantId, CreateReviewDto input);
        Task<List<SpecialOfferDto>> GetRestaurantOffersAsync(Guid restaurantId);
        Task<List<MenuItemDto>> GetRestaurantMenuItemsAsync(Guid restaurantId);
        
        // Admin methods for restaurant owner management
        Task<List<RestaurantDto>> GetRestaurantsByOwnerAsync(string ownerId);
        Task<RestaurantSummaryStatisticsDto> GetRestaurantStatisticsAsync(Guid restaurantId);
    }
    
    public class CreateReviewDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
