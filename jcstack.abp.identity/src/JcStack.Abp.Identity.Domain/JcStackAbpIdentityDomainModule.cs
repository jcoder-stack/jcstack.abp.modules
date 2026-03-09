using Volo.Abp.Identity;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(AbpIdentityDomainModule),
    typeof(JcStackAbpIdentityDomainSharedModule)
)]
public class JcStackAbpIdentityDomainModule : AbpModule
{
}
