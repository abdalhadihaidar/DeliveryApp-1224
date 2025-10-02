using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Application.Services
{
    public class RestaurantCategoryAppService : ApplicationService, IRestaurantCategoryAppService
    {
        private readonly IRestaurantCategoryRepository _categoryRepository;
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;

        public RestaurantCategoryAppService(
            IRestaurantCategoryRepository categoryRepository,
            IRepository<Restaurant, Guid> restaurantRepository)
        {
            _categoryRepository = categoryRepository;
            _restaurantRepository = restaurantRepository;
        }

        // Public methods for all users
        public async Task<List<RestaurantCategoryListDto>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveCategoriesOrderedAsync();
            
            var categoryDtos = new List<RestaurantCategoryListDto>();
            
            foreach (var category in categories)
            {
                var restaurantCount = await _restaurantRepository.CountAsync(r => r.CategoryId == category.Id);
                
                categoryDtos.Add(new RestaurantCategoryListDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Color = category.Color,
                    Icon = category.Icon,
                    IsActive = category.IsActive,
                    SortOrder = category.SortOrder,
                    RestaurantCount = restaurantCount
                });
            }
            
            return categoryDtos;
        }

        public async Task<RestaurantCategoryDto> GetAsync(Guid id)
        {
            var category = await _categoryRepository.GetAsync(id);
            var restaurantCount = await _restaurantRepository.CountAsync(r => r.CategoryId == id);
            
            return new RestaurantCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                Color = category.Color,
                Icon = category.Icon,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder,
                RestaurantCount = restaurantCount,
                CreationTime = category.CreationTime
            };
        }

        // Admin/Manager methods for category management
        [Authorize(Roles = "admin,manager")]
        public async Task<PagedResultDto<RestaurantCategoryDto>> GetListAsync(Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto input)
        {
            var query = await _categoryRepository.GetQueryableAsync();
            
            // Apply sorting
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                // Handle different sorting options
                query = input.Sorting.ToLowerInvariant() switch
                {
                    "name" => query.OrderBy(c => c.Name),
                    "name desc" => query.OrderByDescending(c => c.Name),
                    "sortorder" => query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name),
                    "sortorder desc" => query.OrderByDescending(c => c.SortOrder).ThenBy(c => c.Name),
                    "creationtime" => query.OrderBy(c => c.CreationTime),
                    "creationtime desc" => query.OrderByDescending(c => c.CreationTime),
                    _ => query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                };
            }
            else
            {
                query = query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);
            }

            var totalCount = await query.CountAsync();
            
            var categories = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var categoryDtos = new List<RestaurantCategoryDto>();
            
            foreach (var category in categories)
            {
                var restaurantCount = await _restaurantRepository.CountAsync(r => r.CategoryId == category.Id);
                
                categoryDtos.Add(new RestaurantCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Color = category.Color,
                    Icon = category.Icon,
                    IsActive = category.IsActive,
                    SortOrder = category.SortOrder,
                    RestaurantCount = restaurantCount,
                    CreationTime = category.CreationTime
                });
            }

            return new PagedResultDto<RestaurantCategoryDto>
            {
                TotalCount = totalCount,
                Items = categoryDtos
            };
        }

        [Authorize(Roles = "admin,manager")]
        public async Task<RestaurantCategoryDto> CreateAsync(CreateRestaurantCategoryDto input)
        {
            // Check if name is unique
            var isNameUnique = await _categoryRepository.IsNameUniqueAsync(input.Name);
            if (!isNameUnique)
            {
                throw new UserFriendlyException($"A category with the name '{input.Name}' already exists.");
            }

            var category = new RestaurantCategory
            {
                Name = input.Name,
                Description = input.Description,
                ImageUrl = input.ImageUrl,
                Color = input.Color,
                Icon = input.Icon,
                IsActive = input.IsActive,
                SortOrder = input.SortOrder
            };

            category = await _categoryRepository.InsertAsync(category, autoSave: true);
            
            return new RestaurantCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                Color = category.Color,
                Icon = category.Icon,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder,
                RestaurantCount = 0,
                CreationTime = category.CreationTime
            };
        }

        [Authorize(Roles = "admin,manager")]
        public async Task<RestaurantCategoryDto> UpdateAsync(Guid id, UpdateRestaurantCategoryDto input)
        {
            var category = await _categoryRepository.GetAsync(id);

            // Check if name is unique (excluding current category)
            var isNameUnique = await _categoryRepository.IsNameUniqueAsync(input.Name, id);
            if (!isNameUnique)
            {
                throw new UserFriendlyException($"A category with the name '{input.Name}' already exists.");
            }

            category.Name = input.Name;
            category.Description = input.Description;
            category.ImageUrl = input.ImageUrl;
            category.Color = input.Color;
            category.Icon = input.Icon;
            category.IsActive = input.IsActive;
            category.SortOrder = input.SortOrder;

            category = await _categoryRepository.UpdateAsync(category, autoSave: true);
            
            var restaurantCount = await _restaurantRepository.CountAsync(r => r.CategoryId == id);
            
            return new RestaurantCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                Color = category.Color,
                Icon = category.Icon,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder,
                RestaurantCount = restaurantCount,
                CreationTime = category.CreationTime
            };
        }

        [Authorize(Roles = "admin,manager")]
        public async Task DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetAsync(id);
            
            // Check if any restaurants are using this category
            var restaurantCount = await _restaurantRepository.CountAsync(r => r.CategoryId == id);
            if (restaurantCount > 0)
            {
                throw new UserFriendlyException($"Cannot delete category '{category.Name}' because it is being used by {restaurantCount} restaurant(s). Please move the restaurants to another category first.");
            }

            await _categoryRepository.DeleteAsync(id);
        }

        [Authorize(Roles = "admin,manager")]
        public async Task<RestaurantCategoryDto> SetActiveStatusAsync(Guid id, bool isActive)
        {
            var category = await _categoryRepository.GetAsync(id);
            category.IsActive = isActive;
            
            category = await _categoryRepository.UpdateAsync(category, autoSave: true);
            
            var restaurantCount = await _restaurantRepository.CountAsync(r => r.CategoryId == id);
            
            return new RestaurantCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                Color = category.Color,
                Icon = category.Icon,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder,
                RestaurantCount = restaurantCount,
                CreationTime = category.CreationTime
            };
        }

        [Authorize(Roles = "admin,manager")]
        public async Task<List<RestaurantCategoryDto>> UpdateSortOrderAsync(List<Guid> categoryIds)
        {
            var categories = await _categoryRepository.GetListAsync(c => categoryIds.Contains(c.Id));
            
            for (int i = 0; i < categoryIds.Count; i++)
            {
                var category = categories.FirstOrDefault(c => c.Id == categoryIds[i]);
                if (category != null)
                {
                    category.SortOrder = i;
                }
            }
            
            await _categoryRepository.UpdateManyAsync(categories, autoSave: true);
            
            var updatedCategories = await _categoryRepository.GetListAsync(c => categoryIds.Contains(c.Id));
            var categoryDtos = new List<RestaurantCategoryDto>();
            
            foreach (var category in updatedCategories.OrderBy(c => c.SortOrder))
            {
                var restaurantCount = await _restaurantRepository.CountAsync(r => r.CategoryId == category.Id);
                
                categoryDtos.Add(new RestaurantCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Color = category.Color,
                    Icon = category.Icon,
                    IsActive = category.IsActive,
                    SortOrder = category.SortOrder,
                    RestaurantCount = restaurantCount,
                    CreationTime = category.CreationTime
                });
            }
            
            return categoryDtos;
        }
    }
}
