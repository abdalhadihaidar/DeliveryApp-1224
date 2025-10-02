using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Volo.Abp.Testing;

namespace DeliveryApp
{
    /* This is a non-generic version of DeliveryAppTestBase for use with ICollectionFixture */
    public class DeliveryAppTestBase : DeliveryAppTestBase<DeliveryAppTestBaseModule>
    {
        // Inherits all functionality from the generic base class
    }
}