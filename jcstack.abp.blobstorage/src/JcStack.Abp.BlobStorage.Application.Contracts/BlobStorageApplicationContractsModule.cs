using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace JcStack.Abp.BlobStorage;

[DependsOn(
    typeof(BlobStorageDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class BlobStorageApplicationContractsModule : AbpModule
{

}
