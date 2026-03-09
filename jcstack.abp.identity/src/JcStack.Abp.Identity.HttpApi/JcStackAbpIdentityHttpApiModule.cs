using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using JcStack.Abp.Identity.Localization;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(AbpIdentityHttpApiModule),
    typeof(JcStackAbpIdentityApplicationContractsModule)
)]
public class JcStackAbpIdentityHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(JcStackAbpIdentityHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<JcStackAbpIdentityResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
