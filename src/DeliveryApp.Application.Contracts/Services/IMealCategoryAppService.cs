using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IMealCategoryAppService : IApplicationService
    {
        Task<List<MealCategoryDto>> GetByRestaurantIdAsync(Guid restaurantId, string userId);
        Task<MealCategoryDto> GetAsync(Guid id, string userId);
        Task<MealCategoryDto> CreateAsync(Guid restaurantId, CreateMealCategoryDto input, string userId);
        Task<MealCategoryDto> UpdateAsync(Guid id, UpdateMealCategoryDto input, string userId);
        Task<bool> DeleteAsync(Guid id, string userId);
    }
}
