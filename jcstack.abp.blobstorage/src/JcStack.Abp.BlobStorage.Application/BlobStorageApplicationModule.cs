using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.BlobStoring;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(BlobStorageDomainModule),
    typeof(BlobStorageApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBlobStoringModule)
)]
public class BlobStorageApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<BlobStorageApplicationModule>();

        // 配置 FileStorage 选项
        Configure<BlobStorageOptions>(options =>
        {
            // 默认配置，可在主机项目中覆盖
        });
    }
}
