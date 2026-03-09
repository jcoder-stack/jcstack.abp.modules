using Volo.Abp.Modularity;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(BlobStorageDomainModule),
    typeof(BlobStorageTestBaseModule)
)]
public class BlobStorageDomainTestModule : AbpModule
{

}
