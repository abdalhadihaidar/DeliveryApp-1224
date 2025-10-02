using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Identity;

namespace DeliveryApp.Application.Services
{
    public class AdminRestaurantAppService : ApplicationService, IAdminRestaurantAppService
    {
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<RestaurantCategory, Guid> _categoryRepository;

        public AdminRestaurantAppService(
            IRepository<Restaurant, Guid> restaurantRepository,
            IRepository<AppUser, Guid> userRepository,
            IdentityUserManager userManager,
            IRepository<RestaurantCategory, Guid> categoryRepository)
        {
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _categoryRepository = categoryRepository;
        }

        public async Task<PagedResultDto<RestaurantDto>> GetAllRestaurantsAsync(int page = 1, int pageSize = 10, string sortBy = "id", string sortOrder = "desc")
        {
            var query = await _restaurantRepository.GetQueryableAsync();
            
            // Include related data
            query = query.Include(r => r.Owner)
                        .Include(r => r.Category)
                        .Include(r => r.Address);

            // Apply sorting
            if (sortBy?.ToLower() == "name")
            {
                query = sortOrder?.ToLower() == "asc" 
                    ? query.OrderBy(r => r.Name)
                    : query.OrderByDescending(r => r.Name);
            }
            else if (sortBy?.ToLower() == "createdate")
            {
                query = sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(r => r.CreationTime)
                    : query.OrderByDescending(r => r.CreationTime);
            }
            else
            {
                query = sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(r => r.Id)
                    : query.OrderByDescending(r => r.Id);
            }

            var totalCount = await AsyncExecuter.CountAsync(query);
            var restaurants = await AsyncExecuter.ToListAsync(
                query.Skip((page - 1) * pageSize).Take(pageSize)
            );

            var restaurantDtos = restaurants.Select(r => ObjectMapper.Map<Restaurant, RestaurantDto>(r)).ToList();

            return new PagedResultDto<RestaurantDto>
            {
                TotalCount = totalCount,
                Items = restaurantDtos
            };
        }

        public async Task<List<RestaurantDto>> GetPendingRestaurantsAsync()
        {
            var query = await _restaurantRepository.GetQueryableAsync();
            query = query.Include(r => r.Owner)
                        .Include(r => r.Category)
                        .Include(r => r.Address)
                        .Where(r => r.Tags.Contains("في انتظار الموافقة"));

            var restaurants = await AsyncExecuter.ToListAsync(query);
            return restaurants.Select(r => ObjectMapper.Map<Restaurant, RestaurantDto>(r)).ToList();
        }

        public async Task<RestaurantDto> ApproveRestaurantAsync(Guid restaurantId)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Update restaurant to approved status using the existing pattern
            restaurant.Tags.Remove("في انتظار الموافقة");
            restaurant.Tags.Add("موافق عليه");
            restaurant.IsActive = true;
            
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> RejectRestaurantAsync(Guid restaurantId, string reason)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            // Update restaurant to rejected status using the existing pattern
            restaurant.Tags.Remove("في انتظار الموافقة");
            restaurant.Tags.Add("مرفوض");
            restaurant.Tags.Add($"سبب الرفض: {reason}");
            restaurant.IsActive = false;
            
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> UpdateCommissionAsync(Guid restaurantId, decimal commissionPercent)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            restaurant.CommissionPercent = commissionPercent;
            
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> ToggleActivationAsync(Guid restaurantId, bool isActive)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            restaurant.IsActive = isActive;
            
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> GetRestaurantDetailsAsync(Guid restaurantId)
        {
            var query = await _restaurantRepository.GetQueryableAsync();
            var restaurant = await AsyncExecuter.FirstOrDefaultAsync(
                query.Include(r => r.Owner)
                     .Include(r => r.Category)
                     .Include(r => r.Address)
                     .Where(r => r.Id == restaurantId)
            );

            if (restaurant == null)
            {
                throw new EntityNotFoundException(typeof(Restaurant), restaurantId);
            }

            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        // Restaurant creation and management
        public async Task<RestaurantDto> CreateRestaurantAsync(CreateRestaurantDto input, Guid ownerId)
        {
            var owner = await _userRepository.GetAsync(ownerId);
            
            var restaurant = new Restaurant
            {
                Name = input.Name,
                Description = input.Description,
                ImageUrl = input.ImageUrl,
                CategoryId = input.CategoryId,
                DeliveryFee = input.DeliveryFee,
                MinimumOrderAmount = input.MinimumOrderAmount,
                Tags = input.Tags?.ToList() ?? new List<string>(),
                Address = input.Address != null ? ObjectMapper.Map<AddressDto, Address>(input.Address) : null,
                OwnerId = ownerId,
                IsActive = true,
                CommissionPercent = 0 // Default commission
            };

            await _restaurantRepository.InsertAsync(restaurant);
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> UpdateRestaurantAsync(Guid restaurantId, UpdateRestaurantDto input)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            
            restaurant.Name = input.Name;
            restaurant.Description = input.Description;
            restaurant.ImageUrl = input.ImageUrl;
            restaurant.CategoryId = input.CategoryId;
            restaurant.DeliveryFee = input.DeliveryFee;
            restaurant.MinimumOrderAmount = input.MinimumOrderAmount;
            restaurant.Tags = input.Tags?.ToList() ?? new List<string>();
            restaurant.Address = input.Address != null ? ObjectMapper.Map<AddressDto, Address>(input.Address) : restaurant.Address;

            await _restaurantRepository.UpdateAsync(restaurant);
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<bool> DeleteRestaurantAsync(Guid restaurantId)
        {
            await _restaurantRepository.DeleteAsync(restaurantId);
            return true;
        }

        // Restaurant owner management
        public async Task<RestaurantOwnerDto> CreateRestaurantOwnerAsync(CreateRestaurantOwnerDto input)
        {
            var user = new AppUser(Guid.NewGuid(), input.Name, input.Email)
            {
                Name = input.Name,
                ProfileImageUrl = input.ProfileImageUrl ?? string.Empty,
            };

            // Set phone number using extension method
            user.SetPhoneNumber(input.PhoneNumber, false);

            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                throw new UserFriendlyException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Add to restaurant owner role
            await _userManager.AddToRoleAsync(user, "restaurant_owner");

            return ObjectMapper.Map<AppUser, RestaurantOwnerDto>(user);
        }

        public async Task<RestaurantOwnerDto> UpdateRestaurantOwnerAsync(Guid ownerId, UpdateRestaurantOwnerDto input)
        {
            var user = await _userRepository.GetAsync(ownerId);
            
            user.Name = input.Name;
            user.SetPhoneNumber(input.PhoneNumber, false);
            user.ProfileImageUrl = input.ProfileImageUrl ?? string.Empty;

            await _userRepository.UpdateAsync(user);
            return ObjectMapper.Map<AppUser, RestaurantOwnerDto>(user);
        }

        public async Task<bool> DeleteRestaurantOwnerAsync(Guid ownerId)
        {
            // Check if owner has restaurants
            var hasRestaurants = await _restaurantRepository.AnyAsync(r => r.OwnerId == ownerId);
            if (hasRestaurants)
            {
                throw new UserFriendlyException("Cannot delete restaurant owner who has restaurants. Please reassign restaurants first.");
            }

            await _userRepository.DeleteAsync(ownerId);
            return true;
        }

        public async Task<List<RestaurantOwnerDto>> GetAllRestaurantOwnersAsync()
        {
            var users = await _userRepository.GetListAsync();
            var restaurantOwners = new List<RestaurantOwnerDto>();

            foreach (var user in users)
            {
                var isRestaurantOwner = await _userManager.IsInRoleAsync(user, "restaurant_owner");
                if (isRestaurantOwner)
                {
                    var ownerDto = ObjectMapper.Map<AppUser, RestaurantOwnerDto>(user);
                    
                    // Get restaurant count
                    var restaurantCount = await _restaurantRepository.CountAsync(r => r.OwnerId == user.Id);
                    ownerDto.RestaurantCount = restaurantCount;
                    
                    restaurantOwners.Add(ownerDto);
                }
            }

            return restaurantOwners;
        }

        public async Task<RestaurantOwnerDetailsDto> GetRestaurantOwnerDetailsAsync(Guid ownerId)
        {
            var user = await _userRepository.GetAsync(ownerId);
            var ownerDto = ObjectMapper.Map<AppUser, RestaurantOwnerDetailsDto>(user);
            
            // Get restaurants for this owner
            var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == ownerId);
            ownerDto.Restaurants = restaurants.Select(r => ObjectMapper.Map<Restaurant, RestaurantSummaryDto>(r)).ToList();
            ownerDto.RestaurantCount = restaurants.Count;
            
            return ownerDto;
        }

        // Link restaurant to owner
        public async Task<RestaurantDto> AssignRestaurantToOwnerAsync(Guid restaurantId, Guid ownerId)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            var owner = await _userRepository.GetAsync(ownerId);
            
            restaurant.OwnerId = ownerId;
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> ChangeRestaurantOwnerAsync(Guid restaurantId, Guid newOwnerId)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            var newOwner = await _userRepository.GetAsync(newOwnerId);
            
            restaurant.OwnerId = newOwnerId;
            await _restaurantRepository.UpdateAsync(restaurant);
            
            return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
        }

        // Get restaurants by owner
        public async Task<List<RestaurantDto>> GetRestaurantsByOwnerAsync(Guid ownerId)
        {
            var restaurants = await _restaurantRepository.GetListAsync(r => r.OwnerId == ownerId);
            return restaurants.Select(r => ObjectMapper.Map<Restaurant, RestaurantDto>(r)).ToList();
        }
    }
}
