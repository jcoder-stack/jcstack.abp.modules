using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(JcStackAbpIdentityDomainModule)
)]
public class JcStackAbpIdentityEntityFrameworkCoreModule : AbpModule
{
}
