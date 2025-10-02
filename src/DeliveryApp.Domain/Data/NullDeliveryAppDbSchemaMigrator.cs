using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Data;

/* This is used if database provider does't define
 * IDeliveryAppDbSchemaMigrator implementation.
 */
public class NullDeliveryAppDbSchemaMigrator : IDeliveryAppDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
