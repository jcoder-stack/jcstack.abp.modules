using JcStack.Abp.Message.Localization;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Message.HttpApi;

[DependsOn(
    typeof(MessageApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class MessageHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(MessageHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<MessageResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
