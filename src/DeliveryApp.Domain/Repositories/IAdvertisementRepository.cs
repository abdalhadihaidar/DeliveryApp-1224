using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IAdvertisementRepository : IRepository<Advertisement, Guid>
    {
        Task<List<Advertisement>> GetListAsync(
            int skipCount,
            int maxResultCount,
            string sorting,
            bool includeDetails = false
        );

        Task<List<Advertisement>> GetActiveAdvertisementsAsync(int maxResultCount = 10);
        
        Task<List<Advertisement>> SearchAsync(string searchTerm, int maxResultCount = 10);
        
        Task<List<Advertisement>> GetByLocationAsync(string location, int skipCount = 0, int maxResultCount = 10);
        
        Task<List<Advertisement>> GetByRestaurantAsync(Guid restaurantId, int skipCount = 0, int maxResultCount = 10);
        
        Task<List<Advertisement>> GetExpiredAdvertisementsAsync();
        
        Task<List<Advertisement>> GetUpcomingAdvertisementsAsync();
    }
} 
