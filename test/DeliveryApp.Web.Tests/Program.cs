using Microsoft.AspNetCore.Builder;
using DeliveryApp;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();

builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("DeliveryApp.Web.csproj");
await builder.RunAbpModuleAsync<DeliveryAppWebTestModule>(applicationName: "DeliveryApp.Web" );

public partial class Program
{
}
