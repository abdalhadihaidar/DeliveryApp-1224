using System;
using System.Collections.Generic;
using System.Text;
using DeliveryApp.Localization;
using Volo.Abp.Application.Services;

namespace DeliveryApp;

/* Inherit your application services from this class.
 */
public abstract class DeliveryAppAppService : ApplicationService
{
    protected DeliveryAppAppService()
    {
        LocalizationResource = typeof(DeliveryAppResource);
    }
}
