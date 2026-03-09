using Volo.Abp.Modularity;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(JcStackAbpIdentityEntityFrameworkCoreModule),
    typeof(JcStackAbpIdentityApplicationModule),
    typeof(JcStackAbpIdentityHttpApiModule)
)]
public class JcStackAbpIdentityInstallerModule : AbpModule
{
}
