using System;
using System.IO;
using JcStack.Abp.BlobStorage.StoredFiles;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(BlobStorageApplicationModule),
    typeof(BlobStorageDomainTestModule),
    typeof(AbpBlobStoringFileSystemModule)
)]
public class BlobStorageApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置 FileSystem Blob 存储用于测试
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.Configure<BlobStorageBlobContainer>(container =>
            {
                container.UseFileSystem(fileSystem =>
                {
                    fileSystem.BasePath = Path.Combine(
                        Path.GetTempPath(),
                        "JcStack.Abp.BlobStorage.Tests",
                        Guid.NewGuid().ToString());
                });
            });
        });
    }
}
