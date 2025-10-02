using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Contracts.Services
{
    public interface IAdRequestAppService : IApplicationService
    {
        // Restaurant Owner methods
        Task<AdRequestDto> CreateAsync(Guid restaurantId, CreateAdRequestDto input, string userId);
        Task<AdRequestDto> UpdateAsync(Guid id, UpdateAdRequestDto input, string userId);
        Task<bool> DeleteAsync(Guid id, string userId);
        Task<AdRequestDto> GetAsync(Guid id, string userId);
        Task<List<AdRequestDto>> GetByRestaurantAsync(Guid restaurantId, string userId);

        // Admin methods
        Task<PagedResultDto<AdRequestDto>> GetListAsync(GetAdRequestListDto input);
        Task<List<AdRequestDto>> GetPendingRequestsAsync();
        Task<AdRequestDto> ReviewAsync(Guid id, ReviewAdRequestDto input);
        Task<AdRequestDto> ProcessToAdvertisementAsync(Guid id);
    }
}
