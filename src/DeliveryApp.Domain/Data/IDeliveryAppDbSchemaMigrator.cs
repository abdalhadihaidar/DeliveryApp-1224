using System.Threading.Tasks;

namespace DeliveryApp.Data;

public interface IDeliveryAppDbSchemaMigrator
{
    Task MigrateAsync();
}
