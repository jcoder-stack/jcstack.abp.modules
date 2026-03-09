using Volo.Abp.Identity;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(AbpIdentityApplicationContractsModule),
    typeof(JcStackAbpIdentityDomainSharedModule)
)]
public class JcStackAbpIdentityApplicationContractsModule : AbpModule
{
}
