using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace JcStack.Abp.Keycloak.Identity;

[DependsOn(
    typeof(JcStackAbpKeycloakIdentityApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class JcStackAbpKeycloakIdentityHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(JcStackAbpKeycloakIdentityApplicationContractsModule).Assembly,
            IdentityRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<JcStackAbpKeycloakIdentityHttpApiClientModule>();
        });

    }
}
