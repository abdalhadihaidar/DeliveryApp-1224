using DeliveryApp.Samples;
using Xunit;

namespace DeliveryApp.EntityFrameworkCore.Applications;

[Collection(DeliveryAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<DeliveryAppEntityFrameworkCoreTestModule>
{

}
