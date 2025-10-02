using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using DeliveryApp.EntityFrameworkCore;

namespace DeliveryApp.EntityFrameworkCore.Data
{
    public class DiscriminatorDataMigrationService : ITransientDependency
    {
        public ILogger<DiscriminatorDataMigrationService> Logger { get; set; }

        private readonly DeliveryAppDbContext _dbContext;

        public DiscriminatorDataMigrationService(DeliveryAppDbContext dbContext)
        {
            _dbContext = dbContext;
            Logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscriminatorDataMigrationService>.Instance;
        }

        public async Task FixDiscriminatorValuesAsync()
        {
            try
            {
                Logger.LogInformation("Starting discriminator values fix...");

                // Check if there are users with empty or null discriminator values using raw SQL
                var usersWithEmptyDiscriminator = await _dbContext.Database.SqlQueryRaw<int>(@"
                    SELECT COUNT(*) FROM AbpUsers 
                    WHERE Discriminator IS NULL OR Discriminator = ''").FirstAsync();

                if (usersWithEmptyDiscriminator == 0)
                {
                    Logger.LogInformation("No users with empty discriminator values found. Skipping fix.");
                    return;
                }

                Logger.LogInformation($"Found {usersWithEmptyDiscriminator} users with empty discriminator values. Fixing...");

                // Update users with empty discriminator to 'IdentityUser' by default
                var updatedToIdentityUser = await _dbContext.Database.ExecuteSqlRawAsync(@"
                    UPDATE AbpUsers 
                    SET Discriminator = 'IdentityUser'
                    WHERE Discriminator IS NULL OR Discriminator = ''");

                Logger.LogInformation($"Updated {updatedToIdentityUser} users to have IdentityUser discriminator.");

                // Update users with AppUser-specific properties to 'AppUser'
                var updatedToAppUser = await _dbContext.Database.ExecuteSqlRawAsync(@"
                    UPDATE AbpUsers 
                    SET Discriminator = 'AppUser'
                    WHERE (ProfileImageUrl IS NOT NULL AND ProfileImageUrl != '') 
                       OR (ReviewStatus IS NOT NULL AND ReviewStatus != '') 
                       OR (ReviewReason IS NOT NULL AND ReviewReason != '')");

                Logger.LogInformation($"Updated {updatedToAppUser} users to have AppUser discriminator.");

                // Verify the fix using raw SQL
                var identityUserCount = await _dbContext.Database.SqlQueryRaw<int>(@"
                    SELECT COUNT(*) FROM AbpUsers WHERE Discriminator = 'IdentityUser'").FirstAsync();

                var appUserCount = await _dbContext.Database.SqlQueryRaw<int>(@"
                    SELECT COUNT(*) FROM AbpUsers WHERE Discriminator = 'AppUser'").FirstAsync();

                var emptyDiscriminatorCount = await _dbContext.Database.SqlQueryRaw<int>(@"
                    SELECT COUNT(*) FROM AbpUsers 
                    WHERE Discriminator IS NULL OR Discriminator = ''").FirstAsync();

                Logger.LogInformation($"Discriminator fix completed. IdentityUser: {identityUserCount}, AppUser: {appUserCount}, Empty: {emptyDiscriminatorCount}");

                if (emptyDiscriminatorCount > 0)
                {
                    Logger.LogWarning($"Warning: {emptyDiscriminatorCount} users still have empty discriminator values.");
                }
                else
                {
                    Logger.LogInformation("All users now have proper discriminator values.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fixing discriminator values: {Message}", ex.Message);
                throw;
            }
        }
    }
}
