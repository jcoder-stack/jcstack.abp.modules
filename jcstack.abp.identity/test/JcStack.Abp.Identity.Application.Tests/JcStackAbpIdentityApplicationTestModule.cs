using Volo.Abp.Modularity;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(JcStackAbpIdentityApplicationModule),
    typeof(JcStackAbpIdentityDomainTestModule)
)]
public class JcStackAbpIdentityApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}
