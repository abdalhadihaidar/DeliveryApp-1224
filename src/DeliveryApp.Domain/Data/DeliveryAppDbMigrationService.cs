using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Data;

public class DeliveryAppDbMigrationService : ITransientDependency
{
    public ILogger<DeliveryAppDbMigrationService> Logger { get; set; }

    private readonly IDataSeeder _dataSeeder;
    private readonly IEnumerable<IDeliveryAppDbSchemaMigrator> _dbSchemaMigrators;
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    public DeliveryAppDbMigrationService(
        IDataSeeder dataSeeder,
        IEnumerable<IDeliveryAppDbSchemaMigrator> dbSchemaMigrators,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant)
    {
        _dataSeeder = dataSeeder;
        _dbSchemaMigrators = dbSchemaMigrators;
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;

        Logger = NullLogger<DeliveryAppDbMigrationService>.Instance;
    }

    public async Task MigrateAsync()
    {
        var initialMigrationAdded = AddInitialMigrationIfNotExist();

        if (initialMigrationAdded)
        {
            return;
        }

        Logger.LogInformation("Started database migrations...");

        await MigrateDatabaseSchemaAsync();
        
        // Only fix discriminator values if there are actual issues (check first)
        // await FixDiscriminatorValuesAsync();
        
        await SeedDataAsync();

        Logger.LogInformation($"Successfully completed host database migrations.");

        var tenants = await _tenantRepository.GetListAsync(includeDetails: true);

        var migratedDatabaseSchemas = new HashSet<string>();
        foreach (var tenant in tenants)
        {
            using (_currentTenant.Change(tenant.Id))
            {
                if (tenant.ConnectionStrings.Any())
                {
                    var tenantConnectionStrings = tenant.ConnectionStrings
                        .Select(x => x.Value)
                        .ToList();

                    if (!migratedDatabaseSchemas.IsSupersetOf(tenantConnectionStrings))
                    {
                        await MigrateDatabaseSchemaAsync(tenant);

                        migratedDatabaseSchemas.AddIfNotContains(tenantConnectionStrings);
                    }
                }

                await SeedDataAsync(tenant);
            }

            Logger.LogInformation($"Successfully completed {tenant.Name} tenant database migrations.");
        }

        Logger.LogInformation("Successfully completed all database migrations.");
        Logger.LogInformation("You can safely end this process...");
    }

    private async Task MigrateDatabaseSchemaAsync(Tenant? tenant = null)
    {
        Logger.LogInformation(
            $"Migrating schema for {(tenant == null ? "host" : tenant.Name + " tenant")} database...");

        foreach (var migrator in _dbSchemaMigrators)
        {
            await migrator.MigrateAsync();
        }
    }

    private async Task SeedDataAsync(Tenant? tenant = null)
    {
        Logger.LogInformation($"Executing {(tenant == null ? "host" : tenant.Name + " tenant")} database seed...");

        // Determine if we're in production environment
        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        
        if (isProduction)
        {
            Logger.LogInformation("Production environment detected - using production data seeder");
        }
        else
        {
            Logger.LogInformation("Development environment detected - using sample data seeder");
        }

        try
        {
            await _dataSeeder.SeedAsync(new DataSeedContext(tenant?.Id)
                .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName, IdentityDataSeedContributor.AdminEmailDefaultValue)
                .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName, IdentityDataSeedContributor.AdminPasswordDefaultValue)
            );
        }
        catch (Exception ex) when (ex.Message.Contains("WITH") || ex.Message.Contains("syntax"))
        {
            Logger.LogWarning("Skipping data seeding due to SQL CTE syntax issue. Database will be seeded on next startup.");
            Logger.LogWarning($"Error: {ex.Message}");
        }
    }

    private bool AddInitialMigrationIfNotExist()
    {
        try
        {
            if (!DbMigrationsProjectExists())
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            if (!MigrationsFolderExists())
            {
                AddInitialMigration();
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning("Couldn't determinate if any migrations exist : " + e.Message);
            return false;
        }
    }

    private bool DbMigrationsProjectExists()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

        return dbMigrationsProjectFolder != null;
    }

    private bool MigrationsFolderExists()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();
        return dbMigrationsProjectFolder != null && Directory.Exists(Path.Combine(dbMigrationsProjectFolder, "Migrations"));
    }

    private void AddInitialMigration()
    {
        Logger.LogInformation("Creating initial migration...");

        string argumentPrefix;
        string fileName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            argumentPrefix = "-c";
            fileName = "/bin/bash";
        }
        else
        {
            argumentPrefix = "/C";
            fileName = "cmd.exe";
        }

        var procStartInfo = new ProcessStartInfo(fileName,
            $"{argumentPrefix} \"abp create-migration-and-run-migrator \"{GetEntityFrameworkCoreProjectFolderPath()}\"\""
        );

        try
        {
            Process.Start(procStartInfo);
        }
        catch (Exception)
        {
            throw new Exception("Couldn't run ABP CLI...");
        }
    }

    private string? GetEntityFrameworkCoreProjectFolderPath()
    {
        var slnDirectoryPath = GetSolutionDirectoryPath();

        if (slnDirectoryPath == null)
        {
            throw new Exception("Solution folder not found!");
        }

        var srcDirectoryPath = Path.Combine(slnDirectoryPath, "src");

        return Directory.GetDirectories(srcDirectoryPath)
            .FirstOrDefault(d => d.EndsWith(".EntityFrameworkCore"));
    }

    private string? GetSolutionDirectoryPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory != null && Directory.GetParent(currentDirectory.FullName) != null)
        {
            currentDirectory = Directory.GetParent(currentDirectory.FullName);

            if (currentDirectory != null && Directory.GetFiles(currentDirectory.FullName).FirstOrDefault(f => f.EndsWith(".sln")) != null)
            {
                return currentDirectory.FullName;
            }
        }

        return null;
    }

    private async Task FixDiscriminatorValuesAsync()
    {
        try
        {
            Logger.LogInformation("Checking for discriminator issues...");

            // Get the first schema migrator to access the database context
            var schemaMigrator = _dbSchemaMigrators.FirstOrDefault();
            if (schemaMigrator == null)
            {
                Logger.LogWarning("No database schema migrator found. Skipping discriminator fix.");
                return;
            }

            // First, try to get a private field named "_dbContext" (some migrators keep a direct reference).
            var dbContextField = schemaMigrator.GetType().GetField("_dbContext",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            DbContext? dbContext = null;

            if (dbContextField != null)
            {
                dbContext = dbContextField.GetValue(schemaMigrator) as DbContext;
            }

            // If that failed, attempt to resolve the context via an IServiceProvider field (ABP default implementation).
            if (dbContext == null)
            {
                var serviceProviderField = schemaMigrator.GetType().GetField("_serviceProvider",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (serviceProviderField != null)
                {
                    var serviceProvider = serviceProviderField.GetValue(schemaMigrator) as IServiceProvider;
                    if (serviceProvider != null)
                    {
                        // Try direct resolve first
                        dbContext = serviceProvider.GetService(typeof(DbContext)) as DbContext;

                        // If still null, create a new scope and resolve the concrete DeliveryAppDbContext
                        if (dbContext == null)
                        {
                            using var scope = serviceProvider.CreateScope();
                            dbContext = scope.ServiceProvider.GetService(typeof(DbContext)) as DbContext;
                        }
                    }
                }
            }

            if (dbContext == null)
            {
                Logger.LogWarning("Could not access database context. Skipping discriminator fix.");
                return;
            }

            // Run the discriminator fix SQL directly
            Logger.LogInformation("Running discriminator fix SQL...");
            
            // Update users with empty discriminator to 'IdentityUser' by default
            var updatedToIdentityUser = await dbContext.Database.ExecuteSqlRawAsync(@"
                UPDATE AbpUsers 
                SET Discriminator = 'IdentityUser'
                WHERE Discriminator IS NULL OR Discriminator = ''");

            Logger.LogInformation($"Updated {updatedToIdentityUser} users to have IdentityUser discriminator.");

            // Update users with AppUser-specific properties to 'AppUser'
            var updatedToAppUser = await dbContext.Database.ExecuteSqlRawAsync(@"
                UPDATE AbpUsers 
                SET Discriminator = 'AppUser'
                WHERE (ProfileImageUrl IS NOT NULL AND ProfileImageUrl != '') 
                   OR (ReviewStatus IS NOT NULL AND ReviewStatus != '') 
                   OR (ReviewReason IS NOT NULL AND ReviewReason != '')");

            Logger.LogInformation($"Updated {updatedToAppUser} users to have AppUser discriminator.");

            Logger.LogInformation("Discriminator fix completed successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to fix discriminator values: {Message}", ex.Message);
            Logger.LogInformation("Continuing with data seeding...");
        }
    }
}
