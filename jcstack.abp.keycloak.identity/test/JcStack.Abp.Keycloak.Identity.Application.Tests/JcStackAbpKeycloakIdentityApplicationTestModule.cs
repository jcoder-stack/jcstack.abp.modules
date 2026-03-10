using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

[DependsOn(
    typeof(JcStackAbpKeycloakIdentityApplicationModule),
    typeof(JcStackAbpKeycloakIdentityDomainTestModule)
    )]
public class JcStackAbpKeycloakIdentityApplicationTestModule : AbpModule
{

}
