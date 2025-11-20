using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [Route("api/restaurants")]
    [ApiController]
    public class RestaurantController : AbpController
    {
        private readonly IRestaurantAppService _restaurantAppService;

        public RestaurantController(IRestaurantAppService restaurantAppService)
        {
            _restaurantAppService = restaurantAppService;
        }

        [HttpGet]
        public async Task<List<RestaurantDto>> GetList([FromQuery] GetRestaurantListDto input)
        {
            return await _restaurantAppService.GetListAsync(input);
        }

        [HttpGet("{id}")]
        public async Task<RestaurantDto> Get(Guid id)
        {
            return await _restaurantAppService.GetAsync(id);
        }

        [HttpGet("by-ids")]
        public async Task<List<RestaurantDto>> GetByIds([FromQuery] List<Guid> ids)
        {
            return await _restaurantAppService.GetByIdsAsync(ids);
        }

        [HttpGet("search")]
        public async Task<List<RestaurantDto>> Search([FromQuery] string searchTerm, [FromQuery] int maxResultCount = 10)
        {
            return await _restaurantAppService.SearchAsync(searchTerm, maxResultCount);
        }

        [HttpGet("search-meals")]
        public async Task<List<MenuItemDto>> SearchMeals([FromQuery] string searchTerm, [FromQuery] int maxResultCount = 10)
        {
            return await _restaurantAppService.SearchMealsAsync(searchTerm, maxResultCount);
        }

        [HttpGet("by-category/{category}")]
        public async Task<List<RestaurantDto>> GetByCategory(string category, [FromQuery] int skipCount = 0, [FromQuery] int maxResultCount = 10)
        {
            return await _restaurantAppService.GetByCategoryAsync(category, skipCount, maxResultCount);
        }

        [HttpGet("{restaurantId}/reviews")]
        public async Task<List<ReviewDto>> GetRestaurantReviews(Guid restaurantId)
        {
            return await _restaurantAppService.GetRestaurantReviewsAsync(restaurantId);
        }

        [HttpPost("{restaurantId}/reviews")]
        [Authorize]
        public async Task<ReviewDto> AddReview(Guid restaurantId, [FromBody] CreateReviewDto input)
        {
            return await _restaurantAppService.AddReviewAsync(restaurantId, input);
        }

        [HttpGet("{restaurantId}/offers")]
        public async Task<List<SpecialOfferDto>> GetRestaurantOffers(Guid restaurantId)
        {
            return await _restaurantAppService.GetRestaurantOffersAsync(restaurantId);
        }

        [HttpGet("{restaurantId}/menu")]
        public async Task<List<MenuItemDto>> GetRestaurantMenu(Guid restaurantId)
        {
            return await _restaurantAppService.GetRestaurantMenuItemsAsync(restaurantId);
        }
    }
}
