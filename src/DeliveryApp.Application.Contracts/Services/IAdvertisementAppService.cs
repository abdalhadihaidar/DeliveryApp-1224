using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;

using Volo.Abp.Application.Services;
using Volo.Abp;

namespace DeliveryApp.Application.Services
{
    [RemoteService]
    public interface IAdvertisementAppService : IApplicationService
    {
        Task<PagedResultDto<AdvertisementDto>> GetListAsync(GetAdvertisementListDto input);
        Task<AdvertisementDto> GetAsync(Guid id);
        Task<AdvertisementDto> CreateAsync(CreateAdvertisementDto input);
        Task<AdvertisementDto> UpdateAsync(Guid id, UpdateAdvertisementDto input);
        Task DeleteAsync(Guid id);
        Task<List<AdvertisementSummaryDto>> GetActiveAdvertisementsAsync(int maxResultCount = 10);
        Task<List<AdvertisementSummaryDto>> GetByLocationAsync(string location, int maxResultCount = 10);
        Task<List<AdvertisementSummaryDto>> GetByRestaurantAsync(Guid restaurantId, int maxResultCount = 10);
        Task IncrementClickCountAsync(Guid id);
        Task IncrementViewCountAsync(Guid id);
        Task<List<AdvertisementDto>> GetExpiredAdvertisementsAsync();
        Task<List<AdvertisementDto>> GetUpcomingAdvertisementsAsync();
    }
} 
