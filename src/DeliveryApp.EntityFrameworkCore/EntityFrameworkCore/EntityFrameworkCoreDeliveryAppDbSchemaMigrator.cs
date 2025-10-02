using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DeliveryApp.Data;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.EntityFrameworkCore;

public class EntityFrameworkCoreDeliveryAppDbSchemaMigrator
    : IDeliveryAppDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreDeliveryAppDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the DeliveryAppDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<DeliveryAppDbContext>()
            .Database
            .MigrateAsync();
    }
}
