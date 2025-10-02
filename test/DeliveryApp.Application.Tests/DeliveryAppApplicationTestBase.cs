using Volo.Abp.Modularity;

namespace DeliveryApp;

public abstract class DeliveryAppApplicationTestBase<TStartupModule> : DeliveryAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
