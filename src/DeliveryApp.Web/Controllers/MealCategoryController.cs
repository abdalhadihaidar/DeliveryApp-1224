using System;
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
    [Route("api/app/meal-category")]
    [Authorize(Roles = "admin,manager")]
    public class MealCategoryController : AbpController
    {
        private readonly IMealCategoryAppService _mealCategoryAppService;

        public MealCategoryController(IMealCategoryAppService mealCategoryAppService)
        {
            _mealCategoryAppService = mealCategoryAppService;
        }

        [HttpGet]
        public async Task<Abp.Application.Services.Dto.PagedResultDto<MealCategoryDto>> GetListAsync([FromQuery] int skipCount = 0, [FromQuery] int maxResultCount = 20, [FromQuery] string sorting = null)
        {
            // For now, return empty list since we don't have a proper implementation
            // This is a placeholder to fix the 404 error
            return new Abp.Application.Services.Dto.PagedResultDto<MealCategoryDto>(0, new System.Collections.Generic.List<MealCategoryDto>());
        }

        [HttpGet("active")]
        public async Task<System.Collections.Generic.List<MealCategoryDto>> GetActiveCategoriesAsync()
        {
            // For now, return empty list since we don't have a proper implementation
            // This is a placeholder to fix the 404 error
            return new System.Collections.Generic.List<MealCategoryDto>();
        }

        [HttpGet("{id}")]
        public async Task<MealCategoryDto> GetAsync(Guid id)
        {
            throw new NotImplementedException("Get single meal category not implemented");
        }

        [HttpPost]
        public async Task<MealCategoryDto> CreateAsync([FromBody] CreateMealCategoryDto input)
        {
            throw new NotImplementedException("Create meal category not implemented");
        }

        [HttpPut("{id}")]
        public async Task<MealCategoryDto> UpdateAsync(Guid id, [FromBody] UpdateMealCategoryDto input)
        {
            throw new NotImplementedException("Update meal category not implemented");
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException("Delete meal category not implemented");
        }
    }
}
