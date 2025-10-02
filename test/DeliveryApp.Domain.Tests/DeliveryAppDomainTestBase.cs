using Volo.Abp.Modularity;

namespace DeliveryApp;

/* Inherit from this class for your domain layer tests. */
public abstract class DeliveryAppDomainTestBase<TStartupModule> : DeliveryAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
