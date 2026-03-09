using Localization.Resources.AbpUi;
using JcStack.Abp.BlobStorage.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(BlobStorageApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class BlobStorageHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(BlobStorageHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<BlobStorageResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
