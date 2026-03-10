using JcStack.Abp.Identity;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(JcStackAbpIdentityDomainModule),
    typeof(JcStackAbpKeycloakIdentityDomainSharedModule)
)]
public class JcStackAbpKeycloakIdentityDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置 Keycloak Identity 选项
        Configure<JcStackAbpKeycloakIdentityOptions>(options =>
        {
            // 默认配置，可以在应用层覆盖
        });
    }
}
