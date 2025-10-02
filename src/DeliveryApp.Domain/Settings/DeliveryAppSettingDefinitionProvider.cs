using Volo.Abp.Settings;

namespace DeliveryApp.Settings;

public class DeliveryAppSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(DeliveryAppSettings.MySetting1));
    }
}
