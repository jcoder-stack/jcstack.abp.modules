using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using JcStack.Abp.Identity.Localization;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(AbpIdentityDomainSharedModule)
)]
public class JcStackAbpIdentityDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<JcStackAbpIdentityDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<JcStackAbpIdentityResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/JcStackAbpIdentity");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("JcStackAbpIdentity", typeof(JcStackAbpIdentityResource));
        });
    }
}
