using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace DeliveryApp.Application.Services
{
    [Authorize(Roles = "restaurant_owner")]
    public class MealCategoryAppService : ApplicationService, IMealCategoryAppService
    {
        private readonly IMealCategoryRepository _mealCategoryRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly ICurrentUser _currentUser;

        public MealCategoryAppService(
            IMealCategoryRepository mealCategoryRepository,
            IRestaurantRepository restaurantRepository,
            ICurrentUser currentUser)
        {
            _mealCategoryRepository = mealCategoryRepository;
            _restaurantRepository = restaurantRepository;
            _currentUser = currentUser;
        }

        private async Task<bool> VerifyRestaurantOwnershipAsync(Guid restaurantId, Guid userId)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            return restaurant.OwnerId == userId;
        }

        public async Task<List<MealCategoryDto>> GetByRestaurantIdAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            if (!await VerifyRestaurantOwnershipAsync(restaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            var categories = await _mealCategoryRepository.GetByRestaurantIdOrderedAsync(restaurantId);
            return ObjectMapper.Map<List<MealCategory>, List<MealCategoryDto>>(categories);
        }

        public async Task<MealCategoryDto> GetAsync(Guid id, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var category = await _mealCategoryRepository.GetAsync(id);
            
            if (!await VerifyRestaurantOwnershipAsync(category.RestaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            return ObjectMapper.Map<MealCategory, MealCategoryDto>(category);
        }

        public async Task<MealCategoryDto> CreateAsync(Guid restaurantId, CreateMealCategoryDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            if (!await VerifyRestaurantOwnershipAsync(restaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Check if name is unique within restaurant
            if (!await _mealCategoryRepository.IsNameUniqueInRestaurantAsync(input.Name, restaurantId))
            {
                throw new ArgumentException($"A meal category with name '{input.Name}' already exists in this restaurant.");
            }

            var category = new MealCategory(GuidGenerator.Create())
            {
                RestaurantId = restaurantId,
                Name = input.Name,
                SortOrder = input.SortOrder
            };

            await _mealCategoryRepository.InsertAsync(category);
            return ObjectMapper.Map<MealCategory, MealCategoryDto>(category);
        }

        public async Task<MealCategoryDto> UpdateAsync(Guid id, UpdateMealCategoryDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var category = await _mealCategoryRepository.GetAsync(id);
            
            if (!await VerifyRestaurantOwnershipAsync(category.RestaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Check if name is unique within restaurant (excluding current category)
            if (!await _mealCategoryRepository.IsNameUniqueInRestaurantAsync(input.Name, category.RestaurantId, id))
            {
                throw new ArgumentException($"A meal category with name '{input.Name}' already exists in this restaurant.");
            }

            category.Name = input.Name;
            category.SortOrder = input.SortOrder;

            await _mealCategoryRepository.UpdateAsync(category);
            return ObjectMapper.Map<MealCategory, MealCategoryDto>(category);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var category = await _mealCategoryRepository.GetAsync(id);
            
            if (!await VerifyRestaurantOwnershipAsync(category.RestaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            await _mealCategoryRepository.DeleteAsync(category);
            return true;
        }
    }
}
