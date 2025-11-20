using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace DeliveryApp.HttpApi.Host.Controllers
{
    [RemoteService]
    [Route("api/app/special-offer")]
    public class SpecialOfferController : DeliveryAppController
    {
        private readonly ISpecialOfferAppService _specialOfferAppService;

        public SpecialOfferController(ISpecialOfferAppService specialOfferAppService)
        {
            _specialOfferAppService = specialOfferAppService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<PagedResultDto<SpecialOfferDto>> GetList([FromQuery] GetSpecialOfferListDto input)
        {
            return await _specialOfferAppService.GetListAsync(input);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<SpecialOfferDto> Get(Guid id)
        {
            return await _specialOfferAppService.GetAsync(id);
        }

        [HttpPost]
        [Authorize]
        public async Task<SpecialOfferDto> Create([FromBody] CreateSpecialOfferDto input)
        {
            return await _specialOfferAppService.CreateAsync(input);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<SpecialOfferDto> Update(Guid id, [FromBody] UpdateSpecialOfferDto input)
        {
            return await _specialOfferAppService.UpdateAsync(id, input);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task Delete(Guid id)
        {
            await _specialOfferAppService.DeleteAsync(id);
        }

        [HttpGet("by-status/{restaurantId}/{status}")]
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetByStatus(Guid restaurantId, string status)
        {
            return await _specialOfferAppService.GetOffersByStatusAsync(restaurantId, status);
        }

        [HttpGet("recurring/{restaurantId}")]
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetRecurring(Guid restaurantId)
        {
            return await _specialOfferAppService.GetRecurringOffersAsync(restaurantId);
        }

        [HttpGet("upcoming/{restaurantId}")]
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetUpcoming(Guid restaurantId)
        {
            return await _specialOfferAppService.GetUpcomingOffersAsync(restaurantId);
        }

        [HttpGet("expired/{restaurantId}")]
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetExpired(Guid restaurantId)
        {
            return await _specialOfferAppService.GetExpiredOffersAsync(restaurantId);
        }

        [HttpGet("search/{restaurantId}")]
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> Search(Guid restaurantId, [FromQuery] string term)
        {
            return await _specialOfferAppService.SearchOffersAsync(restaurantId, term);
        }

        [HttpGet("by-category/{restaurantId}/{category}")]
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetByCategory(Guid restaurantId, string category)
        {
            return await _specialOfferAppService.GetOffersByCategoryAsync(restaurantId, category);
        }

        [HttpPost("{id}/activate")]
        [Authorize]
        public async Task<SpecialOfferDto> Activate(Guid id)
        {
            return await _specialOfferAppService.ActivateOfferAsync(id);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize]
        public async Task<SpecialOfferDto> Deactivate(Guid id)
        {
            return await _specialOfferAppService.DeactivateOfferAsync(id);
        }

        [HttpPost("{id}/pause")]
        [Authorize]
        public async Task<SpecialOfferDto> Pause(Guid id)
        {
            return await _specialOfferAppService.PauseOfferAsync(id);
        }

        [HttpPost("{id}/resume")]
        [Authorize]
        public async Task<SpecialOfferDto> Resume(Guid id)
        {
            return await _specialOfferAppService.ResumeOfferAsync(id);
        }

        [HttpPost("{id}/schedule")]
        [Authorize]
        public async Task<SpecialOfferDto> Schedule(Guid id, [FromBody] SpecialOfferScheduleDto scheduleDto)
        {
            return await _specialOfferAppService.ScheduleOfferAsync(id, scheduleDto);
        }

        [HttpGet("active/{restaurantId}")]
        [AllowAnonymous]
        public async Task<List<SpecialOfferDto>> GetActive(Guid restaurantId)
        {
            return await _specialOfferAppService.GetActiveRestaurantOffersAsync(restaurantId);
        }
    }
}

