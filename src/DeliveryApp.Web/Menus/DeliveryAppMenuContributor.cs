using System.Threading.Tasks;
using DeliveryApp.Localization;
using DeliveryApp.MultiTenancy;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace DeliveryApp.Web.Menus;

public class DeliveryAppMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<DeliveryAppResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                DeliveryAppMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fas fa-home",
                order: 0
            )
        );
/*
        // Add Mobile App link
        context.Menu.Items.Insert(
            1,
            new ApplicationMenuItem(
                DeliveryAppMenus.MobileApp,
                l["Menu:MobileApp"],
                "https://play.google.com/store/apps/details?id=com.waseel.app",
                icon: "fas fa-mobile-alt",
                order: 1
            )
        );*/

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        return Task.CompletedTask;
    }
}
