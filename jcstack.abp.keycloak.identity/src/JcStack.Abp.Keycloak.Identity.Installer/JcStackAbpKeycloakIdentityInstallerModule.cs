using JcStack.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// Keycloak Identity 安装器模块
/// 
/// 依赖 JcStack.Abp.Identity.Installer 以获得 OrganizationUnit API
/// </summary>
[DependsOn(
    typeof(AbpVirtualFileSystemModule),
    typeof(JcStackAbpIdentityInstallerModule)
    )]
public class JcStackAbpKeycloakIdentityInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<JcStackAbpKeycloakIdentityInstallerModule>();
        });
    }
}
