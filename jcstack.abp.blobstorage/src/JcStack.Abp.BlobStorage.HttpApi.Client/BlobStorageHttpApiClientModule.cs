using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(BlobStorageApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class BlobStorageHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(BlobStorageApplicationContractsModule).Assembly,
            BlobStorageRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<BlobStorageHttpApiClientModule>();
        });

    }
}
