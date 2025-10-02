using DeliveryApp.Localization;
using Volo.Abp.AspNetCore.Components;

namespace DeliveryApp.Blazor.WebApp;

public abstract class DeliveryAppComponentBase : AbpComponentBase
{
    protected DeliveryAppComponentBase()
    {
        LocalizationResource = typeof(DeliveryAppResource);
    }
}
