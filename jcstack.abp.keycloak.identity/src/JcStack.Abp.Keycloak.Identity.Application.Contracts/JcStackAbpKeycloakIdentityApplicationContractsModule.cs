using JcStack.Abp.Identity;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(JcStackAbpIdentityApplicationContractsModule)
)]
public class JcStackAbpKeycloakIdentityApplicationContractsModule : AbpModule
{

}
