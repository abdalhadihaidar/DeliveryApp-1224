using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IRestaurantCategoryAppService : IApplicationService
    {
        // Public methods for all users
        Task<List<RestaurantCategoryListDto>> GetActiveCategoriesAsync();
        Task<RestaurantCategoryDto> GetAsync(Guid id);

        // Admin/Manager methods for category management
        Task<PagedResultDto<RestaurantCategoryDto>> GetListAsync(Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto input);
        Task<RestaurantCategoryDto> CreateAsync(CreateRestaurantCategoryDto input);
        Task<RestaurantCategoryDto> UpdateAsync(Guid id, UpdateRestaurantCategoryDto input);
        Task DeleteAsync(Guid id);
        Task<RestaurantCategoryDto> SetActiveStatusAsync(Guid id, bool isActive);
        Task<List<RestaurantCategoryDto>> UpdateSortOrderAsync(List<Guid> categoryIds);
    }
}
