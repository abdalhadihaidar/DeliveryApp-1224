using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.ObjectMapping;

namespace DeliveryApp.Web.Controllers.Admin
{
    [RemoteService]
    [Route("api/app/admin/ads")]
    [Authorize(Roles = "admin")]
    public class AdAdminController : AbpController
    {
        private readonly IAdRequestRepository _adRepo;
        private readonly IObjectMapper _mapper;

        public AdAdminController(IAdRequestRepository adRepo, IObjectMapper mapper)
        {
            _adRepo = adRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<Application.Contracts.Dtos.PagedResultDto<AdRequestDto>> GetListAsync([FromQuery] int skipCount = 0, [FromQuery] int maxResultCount = 20, [FromQuery] AdRequestStatus? status = null, [FromQuery] Guid? restaurantId = null)
        {
            var all = await _adRepo.GetQueryableAsync();
            var query = all.Where(a => (status == null || a.Status == status) && (restaurantId == null || a.RestaurantId == restaurantId));
            var total = query.Count();
            var items = query.OrderByDescending(a => a.CreationTime).Skip(skipCount).Take(maxResultCount).ToList();
            var dtos = _mapper.Map<System.Collections.Generic.List<DeliveryApp.Domain.Entities.AdRequest>, System.Collections.Generic.List<AdRequestDto>>(items);
            return new Application.Contracts.Dtos.PagedResultDto<AdRequestDto>(total, dtos);
        }

        [HttpPost("{adRequestId}/approve")]
        public async Task<AdRequestDto> ApproveAsync(Guid adRequestId)
        {
            var ad = await _adRepo.GetAsync(adRequestId);
            ad.Status = AdRequestStatus.Approved;
            await _adRepo.UpdateAsync(ad);
            return _mapper.Map<DeliveryApp.Domain.Entities.AdRequest, AdRequestDto>(ad);
        }

        [HttpPost("{adRequestId}/reject")]
        public async Task<AdRequestDto> RejectAsync(Guid adRequestId, [FromQuery] string reason)
        {
            var ad = await _adRepo.GetAsync(adRequestId);
            ad.Status = AdRequestStatus.Rejected;
            ad.ReviewReason = reason;
            await _adRepo.UpdateAsync(ad);
            return _mapper.Map<DeliveryApp.Domain.Entities.AdRequest, AdRequestDto>(ad);
        }
    }
}

