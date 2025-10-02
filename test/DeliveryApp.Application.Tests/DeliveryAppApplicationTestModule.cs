using Volo.Abp.Modularity;

namespace DeliveryApp;

[DependsOn(
    typeof(DeliveryAppApplicationModule),
    typeof(DeliveryAppDomainTestModule)
)]
public class DeliveryAppApplicationTestModule : AbpModule
{

}
