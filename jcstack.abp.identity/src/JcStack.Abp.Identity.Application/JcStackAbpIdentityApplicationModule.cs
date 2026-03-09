using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(AbpIdentityApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(JcStackAbpIdentityDomainModule),
    typeof(JcStackAbpIdentityApplicationContractsModule)
)]
public class JcStackAbpIdentityApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<JcStackAbpIdentityApplicationModule>();
    }
}
