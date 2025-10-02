using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace DeliveryApp.Pages;

public class Index_Tests : DeliveryAppWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
