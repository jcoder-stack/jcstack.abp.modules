using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace JcStack.Abp.Message.HttpApi.Client;

[DependsOn(
    typeof(MessageApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class MessageHttpApiClientModule : AbpModule
{
    public const string RemoteServiceName = "Message";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(MessageApplicationContractsModule).Assembly,
            RemoteServiceName);

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MessageHttpApiClientModule>();
        });
    }
}
