using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace DeliveryApp.Domain.Repositories
{
    public interface IMealCategoryRepository : IRepository<MealCategory, Guid>
    {
        Task<List<MealCategory>> GetByRestaurantIdAsync(Guid restaurantId);
        Task<List<MealCategory>> GetByRestaurantIdOrderedAsync(Guid restaurantId);
        Task<bool> IsNameUniqueInRestaurantAsync(string name, Guid restaurantId, Guid? excludeId = null);
    }
}
