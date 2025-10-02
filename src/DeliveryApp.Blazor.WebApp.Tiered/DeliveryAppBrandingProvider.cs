using Microsoft.Extensions.Localization;
using DeliveryApp.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace DeliveryApp.Blazor.WebApp.Tiered;

[Dependency(ReplaceServices = true)]
public class DeliveryAppBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<DeliveryAppResource> _localizer;

    public DeliveryAppBrandingProvider(IStringLocalizer<DeliveryAppResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
