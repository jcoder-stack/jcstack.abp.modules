using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainModule),
    typeof(JcStackAbpKeycloakJcStackAbpKeycloakIdentityTestBaseModule)
)]
public class JcStackAbpKeycloakIdentityDomainTestModule : AbpModule
{

}
