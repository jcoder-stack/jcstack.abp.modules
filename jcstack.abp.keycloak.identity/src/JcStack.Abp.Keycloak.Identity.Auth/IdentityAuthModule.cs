using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity.Auth;

/// <summary>
/// Keycloak 认证模块
/// 提供 Keycloak JWT Bearer 认证的默认配置
/// 支持继承模块覆盖配置
/// 
/// 依赖 JcStackAbpKeycloakIdentityInstallerModule 以获得 OrganizationUnit API
/// </summary>
[DependsOn(
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(JcStackAbpKeycloakIdentityDomainModule),
    typeof(JcStackAbpKeycloakIdentityInstallerModule)
)]
public class IdentityAuthModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        // 提供默认配置，使用模块的服务可以通过多种方式覆盖：
        // 1. 在 appsettings.json 中配置 AuthServer 或 Keycloak:Auth 节
        // 2. 在依赖模块中调用 services.AddKeycloakAuthentication(...) 并传入自定义配置
        // 3. 继承 KeycloakJwtBearerEvents 并使用 AddKeycloakAuthentication<TEvents>()
        
        context.Services.AddKeycloakAuthentication(configuration);
    }
}
