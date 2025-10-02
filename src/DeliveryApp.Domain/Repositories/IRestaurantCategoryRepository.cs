using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IRestaurantCategoryRepository : IRepository<RestaurantCategory, Guid>
    {
        Task<List<RestaurantCategory>> GetActiveCategoriesAsync();
        Task<List<RestaurantCategory>> GetActiveCategoriesOrderedAsync();
        Task<RestaurantCategory?> GetByNameAsync(string name);
        Task<List<RestaurantCategory>> GetCategoriesWithRestaurantCountAsync();
        Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    }
}
