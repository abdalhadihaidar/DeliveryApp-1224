using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Identity;

namespace DeliveryApp.Application.Extensions
{
    /// <summary>
    /// Extension helpers for <see cref="IdentityUserManager"/>
    /// </summary>
    public static class IdentityUserManagerExtensions
    {
        /// <summary>
        /// Retrieves a user entity whose <see cref="IdentityUser.PhoneNumber"/> matches the supplied phone number.
        /// </summary>
        /// <param name="manager">The ABP <see cref="IdentityUserManager"/></param>
        /// <param name="phoneNumber">E.164-formatted phone number.</param>
        public static async Task<IdentityUser?> FindByPhoneNumberAsync(this IdentityUserManager manager, string phoneNumber)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;

            // IdentityUserManager exposes IQueryable Users which is backed by EF Core in default template.
            // Use FirstOrDefaultAsync when supported; otherwise fall back to LINQ to Objects.
            var query = manager.Users.Where(u => u.PhoneNumber == phoneNumber);

            if (query is IAsyncEnumerable<IdentityUser>)
            {
                return await query.FirstOrDefaultAsync();
            }

            return query.FirstOrDefault();
        }
    }
} 
