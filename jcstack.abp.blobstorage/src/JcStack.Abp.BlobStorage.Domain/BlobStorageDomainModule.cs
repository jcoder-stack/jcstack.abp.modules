using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(BlobStorageDomainSharedModule)
)]
public class BlobStorageDomainModule : AbpModule
{

}
