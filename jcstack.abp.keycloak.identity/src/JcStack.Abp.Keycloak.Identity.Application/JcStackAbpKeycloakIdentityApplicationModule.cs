using JcStack.Abp.Identity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// Keycloak Identity 应用层模块
/// 
/// 依赖关系：
/// - IdentityKeycloakModule：提供 Keycloak Admin 客户端和数据种子
/// - JcStackAbpKeycloakIdentityApplicationContractsModule：应用层契约
/// - JcStackAbpIdentityApplicationModule：JcStack Identity 应用层（包含 OrganizationUnit API）
/// </summary>
[DependsOn(
    typeof(IdentityKeycloakModule),
    typeof(JcStackAbpKeycloakIdentityApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(JcStackAbpIdentityApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundJobsModule)
)]
public class JcStackAbpKeycloakIdentityApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<JcStackAbpKeycloakIdentityApplicationModule>();
    }
}
