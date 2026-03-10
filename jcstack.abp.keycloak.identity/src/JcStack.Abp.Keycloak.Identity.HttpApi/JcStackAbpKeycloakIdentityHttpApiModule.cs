using JcStack.Abp.Identity;
using JcStack.Abp.Keycloak.Identity.Localization;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

[DependsOn(
    typeof(JcStackAbpKeycloakIdentityApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(JcStackAbpIdentityHttpApiModule)
)]
public class JcStackAbpKeycloakIdentityHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(JcStackAbpKeycloakIdentityHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<JcStackAbpKeycloakIdentityResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
