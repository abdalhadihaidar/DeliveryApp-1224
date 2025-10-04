using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;

namespace DeliveryApp.EntityFrameworkCore.Repositories
{
    public class UserRepository : EfCoreRepository<DeliveryAppDbContext, AppUser, Guid>, IUserRepository
    {
        public UserRepository(IDbContextProvider<DeliveryAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public new async Task<DbContext> GetDbContextAsync()
        {
            return await base.GetDbContextAsync();
        }

        public new async Task<AppUser> GetAsync(Guid id, bool includeDetails = false)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<AppUser>()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public new async Task<AppUser> UpdateAsync(AppUser entity, bool autoSave = false)
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Set<AppUser>().Update(entity);
            if (autoSave)
            {
                await dbContext.SaveChangesAsync();
            }
            return entity;
        }

        public async Task<AppUser> GetByIdWithDetailsAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<AppUser>()
                .Include(u => u.Addresses)
                .Include(u => u.PaymentMethods)
                .Include(u => u.FavoriteRestaurants)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<AppUser> GetUserWithDetailsAsync(Guid userId)
        {
            return await GetByIdWithDetailsAsync(userId);
        }

        public async Task<List<Address>> GetUserAddressesAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<Address>()
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task UnsetDefaultAddressAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            var defaultAddresses = await dbContext.Set<Address>()
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var address in defaultAddresses)
            {
                address.IsDefault = false;
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task AddAddressAsync(Address address)
        {
            var dbContext = await GetDbContextAsync();
            await dbContext.Set<Address>().AddAsync(address);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAddressAsync(Address address)
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Set<Address>().Update(address);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAddressAsync(Address address)
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Set<Address>().Remove(address);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<PaymentMethod>> GetUserPaymentMethodsAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<PaymentMethod>()
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task UnsetDefaultPaymentMethodAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            var defaultPaymentMethods = await dbContext.Set<PaymentMethod>()
                .Where(p => p.UserId == userId && p.IsDefault)
                .ToListAsync();

            foreach (var paymentMethod in defaultPaymentMethods)
            {
                paymentMethod.IsDefault = false;
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task AddPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            var dbContext = await GetDbContextAsync();
            await dbContext.Set<PaymentMethod>().AddAsync(paymentMethod);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Set<PaymentMethod>().Update(paymentMethod);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeletePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Set<PaymentMethod>().Remove(paymentMethod);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetUserFavoriteRestaurantsAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<FavoriteRestaurant>()
                .Where(f => f.UserId == userId)
                .Select(f => f.RestaurantId)
                .ToListAsync();
        }

        public async Task AddFavoriteRestaurantAsync(Guid userId, Guid restaurantId)
        {
            var dbContext = await GetDbContextAsync();
            var favoriteId = Guid.NewGuid();
            var favorite = new FavoriteRestaurant(favoriteId)
            {
                UserId = userId,
                RestaurantId = restaurantId
            };

            await dbContext.Set<FavoriteRestaurant>().AddAsync(favorite);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveFavoriteRestaurantAsync(Guid userId, Guid restaurantId)
        {
            var dbContext = await GetDbContextAsync();
            var favorite = await dbContext.Set<FavoriteRestaurant>()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);

            if (favorite != null)
            {
                dbContext.Set<FavoriteRestaurant>().Remove(favorite);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> IsFavoriteRestaurantAsync(Guid userId, Guid restaurantId)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<FavoriteRestaurant>()
                .AnyAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);
        }

        public async Task<List<AppUser>> GetPagedListAsync(int skipCount, int maxResultCount, string sorting = null, string filter = null)
        {
            var dbContext = await GetDbContextAsync();
            
            // Since AppUser inherits from IdentityUser and uses the same table (AbpUsers),
            // we can query directly from the AppUser DbSet
            var query = dbContext.Set<AppUser>().Where(u => !u.IsDeleted).AsNoTracking()
                        .Select(u => new
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Email = u.Email,
                            Name = u.Name,
                            PhoneNumber = u.PhoneNumber,
                            IsActive = u.IsActive,
                            IsDeleted = u.IsDeleted,
                            CreationTime = u.CreationTime,
                            LastModificationTime = u.LastModificationTime,
                            EmailConfirmed = u.EmailConfirmed,
                            PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                            ProfileImageUrl = u.ProfileImageUrl,
                            ReviewStatus = u.ReviewStatus.ToString(),
                            ReviewReason = u.ReviewReason,
                            IsAdminApproved = u.IsAdminApproved
                        });
            
            // Apply sorting
            if (!string.IsNullOrEmpty(sorting))
            {
                query = sorting.ToLower() switch
                {
                    "name" => query.OrderBy(u => u.Name),
                    "email" => query.OrderBy(u => u.Email),
                    "creationtime" => query.OrderBy(u => u.CreationTime),
                    "name desc" => query.OrderByDescending(u => u.Name),
                    "email desc" => query.OrderByDescending(u => u.Email),
                    "creationtime desc" => query.OrderByDescending(u => u.CreationTime),
                    _ => query.OrderBy(u => u.Name)
                };
            }
            else
            {
                query = query.OrderBy(u => u.Name);
            }
            
            var results = await query.Skip(skipCount).Take(maxResultCount).ToListAsync();
            
            // Convert anonymous objects to AppUser objects
            var appUsers = new List<AppUser>();
            foreach (var result in results)
            {
                var appUser = new AppUser(result.Id, result.UserName, result.Email)
                {
                    Name = result.Name,
                    ProfileImageUrl = result.ProfileImageUrl,
                    ReviewStatus = Enum.TryParse<DeliveryApp.Domain.Enums.ReviewStatus>(result.ReviewStatus, true, out var reviewStatus) ? reviewStatus : DeliveryApp.Domain.Enums.ReviewStatus.Pending,
                    ReviewReason = result.ReviewReason,
                    IsAdminApproved = result.IsAdminApproved
                };
                
                // Set IsActive using reflection since it's inherited from IdentityUser
                var isActiveProperty = typeof(AppUser).GetProperty("IsActive");
                if (isActiveProperty != null)
                {
                    isActiveProperty.SetValue(appUser, result.IsActive);
                }
                
                // Set EmailConfirmed using reflection since it's inherited from IdentityUser
                var emailConfirmedProperty = typeof(AppUser).GetProperty("EmailConfirmed");
                if (emailConfirmedProperty != null)
                {
                    emailConfirmedProperty.SetValue(appUser, result.EmailConfirmed);
                }
                
                // Set PhoneNumberConfirmed using reflection since it's inherited from IdentityUser
                var phoneNumberConfirmedProperty = typeof(AppUser).GetProperty("PhoneNumberConfirmed");
                if (phoneNumberConfirmedProperty != null)
                {
                    phoneNumberConfirmedProperty.SetValue(appUser, result.PhoneNumberConfirmed);
                }
                
                // Set phone number using reflection since it has a private setter
                var phoneNumberProperty = typeof(AppUser).GetProperty("PhoneNumber");
                if (phoneNumberProperty != null && !string.IsNullOrEmpty(result.PhoneNumber))
                {
                    phoneNumberProperty.SetValue(appUser, result.PhoneNumber);
                }
                
                // Set audit properties using reflection since they're protected
                var creationTimeProperty = typeof(AppUser).GetProperty("CreationTime");
                if (creationTimeProperty != null)
                {
                    creationTimeProperty.SetValue(appUser, result.CreationTime);
                }
                
                var lastModificationTimeProperty = typeof(AppUser).GetProperty("LastModificationTime");
                if (lastModificationTimeProperty != null)
                {
                    lastModificationTimeProperty.SetValue(appUser, result.LastModificationTime);
                }
                
                appUsers.Add(appUser);
            }
            
            return appUsers;
        }

        public async Task<AppUser> GetByPhoneNumberAsync(string phoneNumber)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<AppUser>()
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<int> GetCountAsync(string filter = null)
        {
            var dbContext = await GetDbContextAsync();
            
            // Count non-deleted users from AbpUsers table
            var query = dbContext.Set<IdentityUser>().Where(u => !u.IsDeleted).AsQueryable();
            
            // Apply filter if provided
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => 
                    u.Name.Contains(filter) || 
                    u.Email.Contains(filter) || 
                    u.UserName.Contains(filter));
            }
            
            return await query.CountAsync();
        }

        public async Task<AppUser> CreateUserAsync(AppUser user, string password, string role)
        {
            var dbContext = await GetDbContextAsync();
            // You may want to hash the password and set up roles here, or delegate to IdentityUserManager
            await dbContext.Set<AppUser>().AddAsync(user);
            await dbContext.SaveChangesAsync();
            // TODO: Assign role and set password using Identity if needed
            return user;
        }

        public async Task<AppUser> UpdateUserAsync(AppUser user, string role = null)
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Set<AppUser>().Update(user);
            await dbContext.SaveChangesAsync();
            // TODO: Update role if needed
            return user;
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            var user = await dbContext.Set<AppUser>().FindAsync(userId);
            if (user != null)
            {
                dbContext.Set<AppUser>().Remove(user);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<AppUser>> GetUsersByRoleAsync(string roleName)
        {
            var dbContext = await GetDbContextAsync();
            
            // Query users who have the specified role
            var query = from user in dbContext.Set<AppUser>()
                       join userRole in dbContext.Set<IdentityUserRole>()
                           on user.Id equals userRole.UserId
                       join role in dbContext.Set<IdentityRole>()
                           on userRole.RoleId equals role.Id
                       where role.Name == roleName
                       select user;
            
            return await query
                .Include(u => u.CurrentLocation)
                .Include(u => u.DeliveryStatus)
                .ToListAsync();
        }
    }
}
