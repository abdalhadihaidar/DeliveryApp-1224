using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Services;
using DeliveryApp.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;


namespace DeliveryApp.HttpApi.Host.Controllers
{
    [RemoteService]
    [Route("api/app/advertisement")]
    public class AdvertisementController : DeliveryAppController
    {
        private readonly IAdvertisementAppService _advertisementAppService;

        public AdvertisementController(IAdvertisementAppService advertisementAppService)
        {
            _advertisementAppService = advertisementAppService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<PagedResultDto<AdvertisementDto>> GetList([FromQuery] GetAdvertisementListDto input)
        {
            return await _advertisementAppService.GetListAsync(input);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<AdvertisementDto> Get(Guid id)
        {
            return await _advertisementAppService.GetAsync(id);
        }

        [HttpPost]
        [Authorize]
        public async Task<AdvertisementDto> Create(CreateAdvertisementDto input)
        {
            return await _advertisementAppService.CreateAsync(input);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<AdvertisementDto> Update(Guid id, UpdateAdvertisementDto input)
        {
            return await _advertisementAppService.UpdateAsync(id, input);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task Delete(Guid id)
        {
            await _advertisementAppService.DeleteAsync(id);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<List<AdvertisementSummaryDto>> GetActiveAdvertisements([FromQuery] int maxResultCount = 10)
        {
            return await _advertisementAppService.GetActiveAdvertisementsAsync(maxResultCount);
        }

        [HttpGet("location/{location}")]
        [AllowAnonymous]
        public async Task<List<AdvertisementSummaryDto>> GetByLocation(string location, [FromQuery] int maxResultCount = 10)
        {
            return await _advertisementAppService.GetByLocationAsync(location, maxResultCount);
        }

        [HttpGet("restaurant/{restaurantId}")]
        [AllowAnonymous]
        public async Task<List<AdvertisementSummaryDto>> GetByRestaurant(Guid restaurantId, [FromQuery] int maxResultCount = 10)
        {
            return await _advertisementAppService.GetByRestaurantAsync(restaurantId, maxResultCount);
        }

        [HttpPost("{id}/click")]
        [AllowAnonymous]
        public async Task IncrementClickCount(Guid id)
        {
            await _advertisementAppService.IncrementClickCountAsync(id);
        }

        [HttpPost("{id}/view")]
        [AllowAnonymous]
        public async Task IncrementViewCount(Guid id)
        {
            await _advertisementAppService.IncrementViewCountAsync(id);
        }

        [HttpGet("expired")]
        [Authorize]
        public async Task<List<AdvertisementDto>> GetExpiredAdvertisements()
        {
            return await _advertisementAppService.GetExpiredAdvertisementsAsync();
        }

        [HttpGet("upcoming")]
        [Authorize]
        public async Task<List<AdvertisementDto>> GetUpcomingAdvertisements()
        {
            return await _advertisementAppService.GetUpcomingAdvertisementsAsync();
        }
    }
} 
