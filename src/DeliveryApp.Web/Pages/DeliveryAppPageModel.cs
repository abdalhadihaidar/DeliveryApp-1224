using DeliveryApp.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace DeliveryApp.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class DeliveryAppPageModel : AbpPageModel
{
    protected DeliveryAppPageModel()
    {
        LocalizationResourceType = typeof(DeliveryAppResource);
    }
}
