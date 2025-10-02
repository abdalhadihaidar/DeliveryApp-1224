using DeliveryApp.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace DeliveryApp.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(DeliveryAppEntityFrameworkCoreModule),
    typeof(DeliveryAppApplicationContractsModule)
    )]
public class DeliveryAppDbMigratorModule : AbpModule
{
}
