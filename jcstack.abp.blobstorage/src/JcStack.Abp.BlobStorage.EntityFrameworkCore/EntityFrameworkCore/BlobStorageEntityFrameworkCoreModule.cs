using JcStack.Abp.BlobStorage.StoredFiles;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace JcStack.Abp.BlobStorage.EntityFrameworkCore;

[DependsOn(
    typeof(BlobStorageDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class BlobStorageEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<BlobStorageDbContext>(options =>
        {
            options.AddDefaultRepositories<IBlobStorageDbContext>(includeAllEntities: true);

            // 注册自定义仓储
            options.AddRepository<StoredFile, EfCoreStoredFileRepository>();
        });
    }
}
