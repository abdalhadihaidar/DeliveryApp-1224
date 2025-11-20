using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;

namespace DeliveryApp.Application.Services
{
    [Authorize]
    public class RestaurantAppService : ApplicationService, IRestaurantAppService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IRepository<Review, Guid> _reviewRepository;
        private readonly IRepository<SpecialOffer, Guid> _offerRepository;
        private readonly IRepository<MenuItem, Guid> _menuItemRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IStringLocalizer<DeliveryAppResource> _localizer;

        public RestaurantAppService(
            IRestaurantRepository restaurantRepository,
            IRepository<Review, Guid> reviewRepository,
            IRepository<SpecialOffer, Guid> offerRepository,
            IRepository<MenuItem, Guid> menuItemRepository,
            ICurrentUser currentUser,
            IStringLocalizer<DeliveryAppResource> localizer)
        {
            _restaurantRepository = restaurantRepository;
            _reviewRepository = reviewRepository;
            _offerRepository = offerRepository;
            _menuItemRepository = menuItemRepository;
            _currentUser = currentUser;
            _localizer = localizer;
        }

        private Guid GetCurrentUserId()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException(_localizer["General:UnauthorizedAccess"]);
            }
            
            var userId = _currentUser.GetId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException(_localizer["General:UserIdNotFound"]);
            }
            
            return userId;
        }

        [AllowAnonymous]
        public async Task<List<RestaurantDto>> GetListAsync(GetRestaurantListDto input)
        {
            var count = await _restaurantRepository.GetCountAsync();
            var restaurants = await _restaurantRepository.GetListAsync(
                input.SkipCount,
                input.MaxResultCount,
                "Name",
                includeDetails: true
            );

            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        [AllowAnonymous]
        public async Task<RestaurantDto> GetAsync(Guid id)
        {
            var restaurant = await _restaurantRepository.GetAsync(id, includeDetails: true);
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        [AllowAnonymous]
        public async Task<List<RestaurantDto>> GetByIdsAsync(List<Guid> ids)
        {
            var restaurants = await _restaurantRepository.GetListAsync(r => ids.Contains(r.Id));
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        [AllowAnonymous]
        public async Task<List<RestaurantDto>> SearchAsync(string searchTerm, int maxResultCount = 10)
        {
            var restaurants = await _restaurantRepository.SearchAsync(searchTerm, maxResultCount);
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        [AllowAnonymous]
        public async Task<List<MenuItemDto>> SearchMealsAsync(string searchTerm, int maxResultCount = 10)
        {
            var menuItems = await _menuItemRepository.GetListAsync(
                m => m.IsAvailable && 
                     (m.Name.Contains(searchTerm) || 
                      m.Description.Contains(searchTerm) || 
                      (m.MealCategory != null && m.MealCategory.Name.Contains(searchTerm)))
            );
            
            // Limit results manually
            var limitedResults = menuItems.Take(maxResultCount).ToList();
            
            return ObjectMapper.Map<List<MenuItem>, List<MenuItemDto>>(limitedResults);
        }

        [AllowAnonymous]
        public async Task<List<RestaurantDto>> GetByCategoryAsync(string category, int skipCount = 0, int maxResultCount = 10)
        {
            var restaurants = await _restaurantRepository.GetByCategoryAsync(category, skipCount, maxResultCount);
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        [AllowAnonymous]
        public async Task<List<ReviewDto>> GetRestaurantReviewsAsync(Guid restaurantId)
        {
            var reviews = await _reviewRepository.GetListAsync(r => r.RestaurantId == restaurantId);
            return ObjectMapper.Map<List<Review>, List<ReviewDto>>(reviews);
        }

        [Authorize]
        public async Task<ReviewDto> AddReviewAsync(Guid restaurantId, CreateReviewDto input)
        {
            var userId = GetCurrentUserId();
            
            // Check if restaurant exists
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Create new review
            var review = new Review(GuidGenerator.Create())
            {
                RestaurantId = restaurantId,
                UserId = userId,
                Rating = input.Rating,
                Comment = input.Comment,
                Date = DateTime.Now
            };
            
            await _reviewRepository.InsertAsync(review);
            
            // Update restaurant rating
            var allReviews = await _reviewRepository.GetListAsync(r => r.RestaurantId == restaurantId);
            restaurant.Rating = allReviews.Average(r => r.Rating);
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Review, ReviewDto>(review);
        }

        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetRestaurantOffersAsync(Guid restaurantId)
        {
            var now = DateTime.Now;
            var offers = await _offerRepository.GetListAsync(
                o => o.RestaurantId == restaurantId && 
                     o.StartDate <= now && 
                     o.EndDate >= now
            );
            
            return ObjectMapper.Map<List<SpecialOffer>, List<SpecialOfferDto>>(offers);
        }

        [AllowAnonymous]
        public async Task<List<MenuItemDto>> GetRestaurantMenuItemsAsync(Guid restaurantId)
        {
            var menuItems = await _menuItemRepository.GetListAsync(m => m.RestaurantId == restaurantId);
            return ObjectMapper.Map<List<MenuItem>, List<MenuItemDto>>(menuItems);
        }

        [AllowAnonymous]
        public async Task<List<RestaurantDto>> GetRestaurantsByOwnerAsync(string ownerId)
        {
            var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId.ToString() == ownerId);
            return ObjectMapper.Map<List<Restaurant>, List<RestaurantDto>>(restaurants);
        }

        [AllowAnonymous]
        public async Task<RestaurantSummaryStatisticsDto> GetRestaurantStatisticsAsync(Guid restaurantId)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            var reviews = await _reviewRepository.GetListAsync(r => r.RestaurantId == restaurantId);
            var menuItems = await _menuItemRepository.GetListAsync(m => m.RestaurantId == restaurantId);

            // Calculate statistics (mock data for now - would need order repository for real data)
            var statistics = new RestaurantSummaryStatisticsDto
            {
                RestaurantId = restaurantId,
                RestaurantName = restaurant.Name,
                TotalOrders = 0, // Would need order repository
                TotalRevenue = 0.0, // Would need order repository
                AverageOrderValue = 0.0, // Would need order repository
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0,
                TotalReviews = reviews.Count,
                LastOrderDate = DateTime.Now.AddDays(-7), // Mock data
                ActiveMenuItems = menuItems.Count(m => m.IsAvailable),
                TotalMenuItems = menuItems.Count
            };

            return statistics;
        }
    }
}
