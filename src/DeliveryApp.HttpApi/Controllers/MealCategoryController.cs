using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.HttpApi.Controllers
{
    [Route("api/meal-categories")]
    [ApiController]
    [Authorize(Roles = "restaurant_owner")]
    public class MealCategoryController : AbpControllerBase
    {
        private readonly IMealCategoryAppService _mealCategoryAppService;

        public MealCategoryController(IMealCategoryAppService mealCategoryAppService)
        {
            _mealCategoryAppService = mealCategoryAppService;
        }

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<List<MealCategoryDto>> GetByRestaurantIdAsync(Guid restaurantId, [FromQuery] string userId)
        {
            return await _mealCategoryAppService.GetByRestaurantIdAsync(restaurantId, userId);
        }

        [HttpGet("{id}")]
        public async Task<MealCategoryDto> GetAsync(Guid id, [FromQuery] string userId)
        {
            return await _mealCategoryAppService.GetAsync(id, userId);
        }

        [HttpPost("restaurant/{restaurantId}")]
        public async Task<MealCategoryDto> CreateAsync(Guid restaurantId, [FromBody] CreateMealCategoryDto input, [FromQuery] string userId)
        {
            return await _mealCategoryAppService.CreateAsync(restaurantId, input, userId);
        }

        [HttpPut("{id}")]
        public async Task<MealCategoryDto> UpdateAsync(Guid id, [FromBody] UpdateMealCategoryDto input, [FromQuery] string userId)
        {
            return await _mealCategoryAppService.UpdateAsync(id, input, userId);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id, [FromQuery] string userId)
        {
            return await _mealCategoryAppService.DeleteAsync(id, userId);
        }
    }
}
