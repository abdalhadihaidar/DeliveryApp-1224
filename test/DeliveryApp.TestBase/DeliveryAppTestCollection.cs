using Xunit;

namespace DeliveryApp
{
    [CollectionDefinition(DeliveryAppTestConsts.CollectionDefinitionName)]
    public class DeliveryAppTestCollection : ICollectionFixture<DeliveryAppTestBaseModule>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}