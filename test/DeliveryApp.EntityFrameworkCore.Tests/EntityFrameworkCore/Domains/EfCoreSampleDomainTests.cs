using DeliveryApp.Samples;
using Xunit;

namespace DeliveryApp.EntityFrameworkCore.Domains;

[Collection(DeliveryAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<DeliveryAppEntityFrameworkCoreTestModule>
{

}
