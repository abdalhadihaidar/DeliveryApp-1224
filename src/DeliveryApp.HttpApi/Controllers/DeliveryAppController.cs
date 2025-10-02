using DeliveryApp.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class DeliveryAppController : AbpControllerBase
{
    protected DeliveryAppController()
    {
        LocalizationResource = typeof(DeliveryAppResource);
    }
}
