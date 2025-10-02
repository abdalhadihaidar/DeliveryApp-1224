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
    [Route("api/ad-requests")]
    [ApiController]
    public class AdRequestController : AbpControllerBase
    {
        private readonly IAdRequestAppService _adRequestAppService;

        public AdRequestController(IAdRequestAppService adRequestAppService)
        {
            _adRequestAppService = adRequestAppService;
        }

        #region Restaurant Owner Endpoints

        [HttpPost("restaurant/{restaurantId}")]
        [Authorize(Roles = "restaurant_owner")]
        public async Task<AdRequestDto> CreateAsync(Guid restaurantId, [FromBody] CreateAdRequestDto input, [FromQuery] string userId)
        {
            return await _adRequestAppService.CreateAsync(restaurantId, input, userId);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "restaurant_owner")]
        public async Task<AdRequestDto> UpdateAsync(Guid id, [FromBody] UpdateAdRequestDto input, [FromQuery] string userId)
        {
            return await _adRequestAppService.UpdateAsync(id, input, userId);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "restaurant_owner")]
        public async Task<bool> DeleteAsync(Guid id, [FromQuery] string userId)
        {
            return await _adRequestAppService.DeleteAsync(id, userId);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "restaurant_owner")]
        public async Task<AdRequestDto> GetAsync(Guid id, [FromQuery] string userId)
        {
            return await _adRequestAppService.GetAsync(id, userId);
        }

        [HttpGet("restaurant/{restaurantId}")]
        [Authorize(Roles = "restaurant_owner")]
        public async Task<List<AdRequestDto>> GetByRestaurantAsync(Guid restaurantId, [FromQuery] string userId)
        {
            return await _adRequestAppService.GetByRestaurantAsync(restaurantId, userId);
        }

        #endregion

        #region Admin Endpoints

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<PagedResultDto<AdRequestDto>> GetListAsync([FromQuery] GetAdRequestListDto input)
        {
            return await _adRequestAppService.GetListAsync(input);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "admin")]
        public async Task<List<AdRequestDto>> GetPendingRequestsAsync()
        {
            return await _adRequestAppService.GetPendingRequestsAsync();
        }

        [HttpPost("{id}/review")]
        [Authorize(Roles = "admin")]
        public async Task<AdRequestDto> ReviewAsync(Guid id, [FromBody] ReviewAdRequestDto input)
        {
            return await _adRequestAppService.ReviewAsync(id, input);
        }

        [HttpPost("{id}/process")]
        [Authorize(Roles = "admin")]
        public async Task<AdRequestDto> ProcessToAdvertisementAsync(Guid id)
        {
            return await _adRequestAppService.ProcessToAdvertisementAsync(id);
        }

        #endregion
    }
}
