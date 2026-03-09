using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class BlobStorageInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<BlobStorageInstallerModule>();
        });
    }
}
