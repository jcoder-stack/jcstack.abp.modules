using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Message;

[DependsOn(
    typeof(MessageDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
)]
public class MessageApplicationContractsModule : AbpModule
{
}
