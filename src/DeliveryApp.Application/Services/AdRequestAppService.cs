using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;

using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace DeliveryApp.Application.Services
{
    public class AdRequestAppService : ApplicationService, IAdRequestAppService
    {
        private readonly IAdRequestRepository _adRequestRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IRepository<Advertisement, Guid> _advertisementRepository;
        private readonly ICurrentUser _currentUser;

        public AdRequestAppService(
            IAdRequestRepository adRequestRepository,
            IRestaurantRepository restaurantRepository,
            IRepository<Advertisement, Guid> advertisementRepository,
            ICurrentUser currentUser)
        {
            _adRequestRepository = adRequestRepository;
            _restaurantRepository = restaurantRepository;
            _advertisementRepository = advertisementRepository;
            _currentUser = currentUser;
        }

        private async Task<bool> VerifyRestaurantOwnershipAsync(Guid restaurantId, Guid userId)
        {
            var restaurant = await _restaurantRepository.GetAsync(restaurantId);
            return restaurant.OwnerId == userId;
        }

        #region Restaurant Owner Methods

        [Authorize(Roles = "restaurant_owner")]
        public async Task<AdRequestDto> CreateAsync(Guid restaurantId, CreateAdRequestDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            if (!await VerifyRestaurantOwnershipAsync(restaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            if (!input.ValidateDates())
            {
                throw new UserFriendlyException("Invalid date range. Start date must be today or later and end date must be after start date.");
            }

            var adRequest = new AdRequest(GuidGenerator.Create())
            {
                RestaurantId = restaurantId,
                Title = input.Title,
                ImageUrl = input.ImageUrl,
                Description = input.Description,
                LinkUrl = input.LinkUrl,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Priority = input.Priority,
                TargetAudience = input.TargetAudience,
                Location = input.Location,
                Budget = input.Budget,
                Status = AdRequestStatus.Pending
            };

            await _adRequestRepository.InsertAsync(adRequest);
            return ObjectMapper.Map<AdRequest, AdRequestDto>(adRequest);
        }

        [Authorize(Roles = "restaurant_owner")]
        public async Task<AdRequestDto> UpdateAsync(Guid id, UpdateAdRequestDto input, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var adRequest = await _adRequestRepository.GetAsync(id);

            if (!await VerifyRestaurantOwnershipAsync(adRequest.RestaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Only allow editing pending requests
            if (adRequest.Status != AdRequestStatus.Pending)
            {
                throw new UserFriendlyException("Can only edit pending ad requests.");
            }

            if (!input.ValidateDates())
            {
                throw new UserFriendlyException("Invalid date range. Start date must be today or later and end date must be after start date.");
            }

            adRequest.Title = input.Title;
            adRequest.ImageUrl = input.ImageUrl;
            adRequest.Description = input.Description;
            adRequest.LinkUrl = input.LinkUrl;
            adRequest.StartDate = input.StartDate;
            adRequest.EndDate = input.EndDate;
            adRequest.Priority = input.Priority;
            adRequest.TargetAudience = input.TargetAudience;
            adRequest.Location = input.Location;
            adRequest.Budget = input.Budget;

            await _adRequestRepository.UpdateAsync(adRequest);
            return ObjectMapper.Map<AdRequest, AdRequestDto>(adRequest);
        }

        [Authorize(Roles = "restaurant_owner")]
        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var adRequest = await _adRequestRepository.GetAsync(id);

            if (!await VerifyRestaurantOwnershipAsync(adRequest.RestaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            // Only allow deleting pending requests
            if (adRequest.Status != AdRequestStatus.Pending)
            {
                throw new UserFriendlyException("Can only delete pending ad requests.");
            }

            await _adRequestRepository.DeleteAsync(id);
            return true;
        }

        [Authorize(Roles = "restaurant_owner")]
        public async Task<AdRequestDto> GetAsync(Guid id, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            var adRequest = await _adRequestRepository.GetAsync(id);

            if (!await VerifyRestaurantOwnershipAsync(adRequest.RestaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            return ObjectMapper.Map<AdRequest, AdRequestDto>(adRequest);
        }

        [Authorize(Roles = "restaurant_owner")]
        public async Task<List<AdRequestDto>> GetByRestaurantAsync(Guid restaurantId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format");
            }

            if (!await VerifyRestaurantOwnershipAsync(restaurantId, userGuid))
            {
                throw new UnauthorizedAccessException("You do not own this restaurant.");
            }

            var adRequests = await _adRequestRepository.GetByRestaurantIdAsync(restaurantId);
            return ObjectMapper.Map<List<AdRequest>, List<AdRequestDto>>(adRequests);
        }

        #endregion

        #region Admin Methods

        [Authorize(Roles = "admin")]
        public async Task<PagedResultDto<AdRequestDto>> GetListAsync(GetAdRequestListDto input)
        {
            var status = !string.IsNullOrEmpty(input.Status) && Enum.TryParse<AdRequestStatus>(input.Status, out var parsedStatus) 
                ? parsedStatus : (AdRequestStatus?)null;

            var adRequests = await _adRequestRepository.GetListWithRestaurantAsync(
                input.SkipCount, 
                input.MaxResultCount, 
                input.SearchTerm, 
                status);

            var totalCount = await _adRequestRepository.GetCountAsync();

            var dtoList = ObjectMapper.Map<List<AdRequest>, List<AdRequestDto>>(adRequests);
            return new PagedResultDto<AdRequestDto>(totalCount, dtoList);
        }

        [Authorize(Roles = "admin")]
        public async Task<List<AdRequestDto>> GetPendingRequestsAsync()
        {
            var pendingRequests = await _adRequestRepository.GetPendingRequestsAsync();
            return ObjectMapper.Map<List<AdRequest>, List<AdRequestDto>>(pendingRequests);
        }

        [Authorize(Roles = "admin")]
        public async Task<AdRequestDto> ReviewAsync(Guid id, ReviewAdRequestDto input)
        {
            var adRequest = await _adRequestRepository.GetAsync(id);

            if (adRequest.Status != AdRequestStatus.Pending)
            {
                throw new UserFriendlyException("Only pending ad requests can be reviewed.");
            }

            var adminUserId = _currentUser.GetId();
            if (adminUserId == null)
            {
                throw new UnauthorizedAccessException("Admin user ID not found.");
            }

            if (input.Approve)
            {
                adRequest.Approve(adminUserId, input.ReviewReason);
            }
            else
            {
                if (string.IsNullOrEmpty(input.ReviewReason))
                {
                    throw new UserFriendlyException("Review reason is required for rejection.");
                }
                adRequest.Reject(adminUserId, input.ReviewReason);
            }

            await _adRequestRepository.UpdateAsync(adRequest);
            return ObjectMapper.Map<AdRequest, AdRequestDto>(adRequest);
        }

        [Authorize(Roles = "admin")]
        public async Task<AdRequestDto> ProcessToAdvertisementAsync(Guid id)
        {
            var adRequest = await _adRequestRepository.GetAsync(id);

            if (adRequest.Status != AdRequestStatus.Approved)
            {
                throw new UserFriendlyException("Only approved ad requests can be processed to advertisements.");
            }

            // Create advertisement from approved ad request
            var advertisement = new Advertisement(GuidGenerator.Create())
            {
                Title = adRequest.Title,
                ImageUrl = adRequest.ImageUrl,
                Description = adRequest.Description,
                LinkUrl = adRequest.LinkUrl,
                StartDate = adRequest.StartDate,
                EndDate = adRequest.EndDate,
                Priority = adRequest.Priority,
                TargetAudience = adRequest.TargetAudience,
                Location = adRequest.Location,
                RestaurantId = adRequest.RestaurantId,
                IsActive = true,
                CreatedById = adRequest.ReviewedById
            };

            await _advertisementRepository.InsertAsync(advertisement);

            // Mark ad request as processed
            adRequest.MarkAsProcessed(advertisement.Id);
            await _adRequestRepository.UpdateAsync(adRequest);

            return ObjectMapper.Map<AdRequest, AdRequestDto>(adRequest);
        }

        #endregion
    }
}
