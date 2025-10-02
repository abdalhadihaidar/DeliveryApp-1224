using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DeliveryApp.Blazor.WebApp.Tiered.Components.Toolbar.LoginLink;
using Volo.Abp.Users;
using Volo.Abp.AspNetCore.Components.Web.Theming.Toolbars;

namespace DeliveryApp.Blazor.WebApp.Tiered.Menus;

public class DeliveryAppToolbarContributor : IToolbarContributor
{
    public virtual Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
    {
        if (context.Toolbar.Name != StandardToolbars.Main)
        {
            return Task.CompletedTask;
        }

        if (!context.ServiceProvider.GetRequiredService<ICurrentUser>().IsAuthenticated)
        {
            context.Toolbar.Items.Add(new ToolbarItem(typeof(LoginLinkViewComponent)));
        }

        return Task.CompletedTask;
    }
}
