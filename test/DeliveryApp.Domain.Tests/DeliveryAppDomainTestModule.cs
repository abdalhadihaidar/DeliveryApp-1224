using Volo.Abp.Modularity;

namespace DeliveryApp;

[DependsOn(
    typeof(DeliveryAppDomainModule),
    typeof(DeliveryAppTestBaseModule)
)]
public class DeliveryAppDomainTestModule : AbpModule
{

}
