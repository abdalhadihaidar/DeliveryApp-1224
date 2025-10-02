using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IAdRequestRepository : IRepository<AdRequest, Guid>
    {
        Task<List<AdRequest>> GetByRestaurantIdAsync(Guid restaurantId);
        Task<List<AdRequest>> GetByStatusAsync(AdRequestStatus status);
        Task<List<AdRequest>> GetPendingRequestsAsync();
        Task<List<AdRequest>> GetListWithRestaurantAsync(int skipCount, int maxResultCount, string? searchTerm = null, AdRequestStatus? status = null);
    }
}
