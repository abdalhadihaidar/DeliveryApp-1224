using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Web.Controllers
{
    [RemoteService]
    [Route("api/app/restaurant-category")]
    [Authorize(Roles = "admin,manager")]
    public class RestaurantCategoryController : AbpController
    {
        private readonly IRestaurantCategoryAppService _restaurantCategoryAppService;

        public RestaurantCategoryController(IRestaurantCategoryAppService restaurantCategoryAppService)
        {
            _restaurantCategoryAppService = restaurantCategoryAppService;
        }

        [HttpGet]
        public async Task<Application.Contracts.Dtos.PagedResultDto<RestaurantCategoryDto>> GetListAsync([FromQuery] int skipCount = 0, [FromQuery] int maxResultCount = 20, [FromQuery] string sorting = null)
        {
            var input = new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
            {
                SkipCount = skipCount,
                MaxResultCount = maxResultCount,
                Sorting = sorting
            };
            
            return await _restaurantCategoryAppService.GetListAsync(input);
        }

        [HttpGet("active")]
        public async Task<List<RestaurantCategoryListDto>> GetActiveCategoriesAsync()
        {
            return await _restaurantCategoryAppService.GetActiveCategoriesAsync();
        }

        [HttpGet("{id}")]
        public async Task<RestaurantCategoryDto> GetAsync(Guid id)
        {
            return await _restaurantCategoryAppService.GetAsync(id);
        }

        [HttpPost]
        public async Task<RestaurantCategoryDto> CreateAsync([FromBody] CreateRestaurantCategoryDto input)
        {
            return await _restaurantCategoryAppService.CreateAsync(input);
        }

        [HttpPut("{id}")]
        public async Task<RestaurantCategoryDto> UpdateAsync(Guid id, [FromBody] UpdateRestaurantCategoryDto input)
        {
            return await _restaurantCategoryAppService.UpdateAsync(id, input);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _restaurantCategoryAppService.DeleteAsync(id);
        }

        [HttpPut("{id}/status")]
        public async Task<RestaurantCategoryDto> SetActiveStatusAsync(Guid id, [FromBody] SetActiveStatusDto input)
        {
            return await _restaurantCategoryAppService.SetActiveStatusAsync(id, input.IsActive);
        }

        [HttpPut("sort-order")]
        public async Task<List<RestaurantCategoryDto>> UpdateSortOrderAsync([FromBody] List<Guid> categoryIds)
        {
            return await _restaurantCategoryAppService.UpdateSortOrderAsync(categoryIds);
        }
    }

    public class SetActiveStatusDto
    {
        public bool IsActive { get; set; }
    }
}
