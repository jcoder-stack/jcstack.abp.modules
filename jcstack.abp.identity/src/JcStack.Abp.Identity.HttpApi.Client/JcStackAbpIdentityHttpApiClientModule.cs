using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(AbpHttpClientModule),
    typeof(JcStackAbpIdentityApplicationContractsModule)
)]
public class JcStackAbpIdentityHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(JcStackAbpIdentityApplicationContractsModule).Assembly,
            JcStackAbpIdentityRemoteServiceConsts.RemoteServiceName);

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<JcStackAbpIdentityHttpApiClientModule>();
        });
    }
}
