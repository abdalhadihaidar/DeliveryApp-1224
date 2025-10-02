using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace DeliveryApp.Application.Services
{
    
    public class AdvertisementAppService : ApplicationService, IAdvertisementAppService
    {
        private readonly IAdvertisementRepository _advertisementRepository;
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly ICurrentUser _currentUser;

        public AdvertisementAppService(
            IAdvertisementRepository advertisementRepository,
            IRepository<Restaurant, Guid> restaurantRepository,
            IRepository<AppUser, Guid> userRepository,
            ICurrentUser currentUser)
        {
            _advertisementRepository = advertisementRepository;
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        public async Task<PagedResultDto<AdvertisementDto>> GetListAsync(GetAdvertisementListDto input)
        {
            var advertisements = await _advertisementRepository.GetListAsync(
                input.SkipCount,
                input.MaxResultCount,
                "CreationTime",
                includeDetails: true
            );

            // Apply filters after getting the list
            var filteredAdvertisements = advertisements.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(input.SearchTerm))
            {
                filteredAdvertisements = filteredAdvertisements.Where(a => a.Title.Contains(input.SearchTerm) || 
                                        (a.Description != null && a.Description.Contains(input.SearchTerm)));
            }
            
            if (input.IsActive.HasValue)
            {
                filteredAdvertisements = filteredAdvertisements.Where(a => a.IsActive == input.IsActive.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(input.Location))
            {
                filteredAdvertisements = filteredAdvertisements.Where(a => a.Location == input.Location);
            }
            
            if (input.RestaurantId.HasValue)
            {
                filteredAdvertisements = filteredAdvertisements.Where(a => a.RestaurantId == input.RestaurantId.Value);
            }
            
            if (input.StartDate.HasValue)
            {
                filteredAdvertisements = filteredAdvertisements.Where(a => a.StartDate >= input.StartDate.Value);
            }
            
            if (input.EndDate.HasValue)
            {
                filteredAdvertisements = filteredAdvertisements.Where(a => a.EndDate <= input.EndDate.Value);
            }

            var totalCount = filteredAdvertisements.Count();
            var finalAdvertisements = filteredAdvertisements
                .OrderByDescending(a => a.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var advertisementDtos = ObjectMapper.Map<List<Advertisement>, List<AdvertisementDto>>(finalAdvertisements);

                         // Populate additional data
             foreach (var dto in advertisementDtos)
             {
                 if (dto.RestaurantId.HasValue)
                 {
                     var restaurant = await _restaurantRepository.GetAsync(dto.RestaurantId.Value);
                     dto.RestaurantName = restaurant.Name;
                 }

                 // Try to get creator information, but handle case where user might not exist
                 if (dto.CreatedById.HasValue)
                 {
                     try
                     {
                         var creator = await _userRepository.GetAsync(dto.CreatedById.Value);
                         dto.CreatedByName = creator.UserName ?? creator.Name ?? creator.Email;
                     }
                     catch (Volo.Abp.Domain.Entities.EntityNotFoundException)
                     {
                         // If user doesn't exist, use a default value
                         dto.CreatedByName = "Unknown User";
                         Console.WriteLine($"Warning: User with ID {dto.CreatedById.Value} not found in database.");
                     }
                 }
                 else
                 {
                     dto.CreatedByName = "System";
                 }
             }

            return new PagedResultDto<AdvertisementDto>
            {
                TotalCount = totalCount,
                Items = advertisementDtos
            };
        }

                 public async Task<AdvertisementDto> GetAsync(Guid id)
         {
             var advertisement = await _advertisementRepository.GetAsync(id);
             var dto = ObjectMapper.Map<Advertisement, AdvertisementDto>(advertisement);

             if (dto.RestaurantId.HasValue)
             {
                 var restaurant = await _restaurantRepository.GetAsync(dto.RestaurantId.Value);
                 dto.RestaurantName = restaurant.Name;
             }

             // Try to get creator information, but handle case where user might not exist
             if (dto.CreatedById.HasValue)
             {
                 try
                 {
                     var creator = await _userRepository.GetAsync(dto.CreatedById.Value);
                     dto.CreatedByName = creator.UserName ?? creator.Name ?? creator.Email;
                 }
                 catch (Volo.Abp.Domain.Entities.EntityNotFoundException)
                 {
                     // If user doesn't exist, use a default value
                     dto.CreatedByName = "Unknown User";
                     Console.WriteLine($"Warning: User with ID {dto.CreatedById.Value} not found in database.");
                 }
             }
             else
             {
                 dto.CreatedByName = "System";
             }

             return dto;
         }

                 public async Task<AdvertisementDto> CreateAsync(CreateAdvertisementDto input)
         {
             // For advertisement creation, we don't need to track the creator
             // Just create the advertisement without user tracking
             var advertisement = new Advertisement(GuidGenerator.Create())
             {
                 Title = input.Title,
                 ImageUrl = input.ImageUrl,
                 Description = input.Description,
                 LinkUrl = input.LinkUrl,
                 StartDate = input.StartDate,
                 EndDate = input.EndDate,
                 IsActive = input.IsActive,
                 Priority = input.Priority,
                 TargetAudience = input.TargetAudience,
                 Location = input.Location,
                 RestaurantId = input.RestaurantId,
                 CreatedById = null // Don't track who created it
             };

            await _advertisementRepository.InsertAsync(advertisement);
            
            // Return the created advertisement directly instead of fetching it again
            var dto = ObjectMapper.Map<Advertisement, AdvertisementDto>(advertisement);
            
                         // Populate additional data
             if (dto.RestaurantId.HasValue)
             {
                 var restaurant = await _restaurantRepository.GetAsync(dto.RestaurantId.Value);
                 dto.RestaurantName = restaurant.Name;
             }

             // Since we're not tracking the creator, just set it to System
             dto.CreatedByName = "System";
            
            return dto;
        }

        public async Task<AdvertisementDto> UpdateAsync(Guid id, UpdateAdvertisementDto input)
        {
            var advertisement = await _advertisementRepository.GetAsync(id);
            
            advertisement.Title = input.Title;
            advertisement.ImageUrl = input.ImageUrl;
            advertisement.Description = input.Description;
            advertisement.LinkUrl = input.LinkUrl;
            advertisement.StartDate = input.StartDate;
            advertisement.EndDate = input.EndDate;
            advertisement.IsActive = input.IsActive;
            advertisement.Priority = input.Priority;
            advertisement.TargetAudience = input.TargetAudience;
            advertisement.Location = input.Location;
            advertisement.RestaurantId = input.RestaurantId;

            await _advertisementRepository.UpdateAsync(advertisement);
            
            return await GetAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _advertisementRepository.DeleteAsync(id);
        }

        [AllowAnonymous]
        public async Task<List<AdvertisementSummaryDto>> GetActiveAdvertisementsAsync(int maxResultCount = 10)
        {
            var now = DateTime.Now;
            var advertisements = await _advertisementRepository.GetActiveAdvertisementsAsync(maxResultCount);
            
            var activeAds = advertisements
                .Where(a => a.IsActive && a.StartDate <= now && a.EndDate >= now)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreationTime)
                .Take(maxResultCount)
                .ToList();

            return ObjectMapper.Map<List<Advertisement>, List<AdvertisementSummaryDto>>(activeAds);
        }

        [AllowAnonymous]
        public async Task<List<AdvertisementSummaryDto>> GetByLocationAsync(string location, int maxResultCount = 10)
        {
            var advertisements = await _advertisementRepository.GetByLocationAsync(location, 0, maxResultCount);
            return ObjectMapper.Map<List<Advertisement>, List<AdvertisementSummaryDto>>(advertisements);
        }

        [AllowAnonymous]
        public async Task<List<AdvertisementSummaryDto>> GetByRestaurantAsync(Guid restaurantId, int maxResultCount = 10)
        {
            var advertisements = await _advertisementRepository.GetByRestaurantAsync(restaurantId, 0, maxResultCount);
            return ObjectMapper.Map<List<Advertisement>, List<AdvertisementSummaryDto>>(advertisements);
        }

        [AllowAnonymous]
        public async Task IncrementClickCountAsync(Guid id)
        {
            var advertisement = await _advertisementRepository.GetAsync(id);
            advertisement.ClickCount = (advertisement.ClickCount ?? 0) + 1;
            await _advertisementRepository.UpdateAsync(advertisement);
        }

        [AllowAnonymous]
        public async Task IncrementViewCountAsync(Guid id)
        {
            var advertisement = await _advertisementRepository.GetAsync(id);
            advertisement.ViewCount = (advertisement.ViewCount ?? 0) + 1;
            await _advertisementRepository.UpdateAsync(advertisement);
        }

        public async Task<List<AdvertisementDto>> GetExpiredAdvertisementsAsync()
        {
            var advertisements = await _advertisementRepository.GetExpiredAdvertisementsAsync();
            return ObjectMapper.Map<List<Advertisement>, List<AdvertisementDto>>(advertisements);
        }

        public async Task<List<AdvertisementDto>> GetUpcomingAdvertisementsAsync()
        {
            var advertisements = await _advertisementRepository.GetUpcomingAdvertisementsAsync();
            return ObjectMapper.Map<List<Advertisement>, List<AdvertisementDto>>(advertisements);
        }
    }
} 
