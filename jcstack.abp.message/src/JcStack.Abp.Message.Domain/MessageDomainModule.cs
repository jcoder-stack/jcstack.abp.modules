using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Message;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(MessageDomainSharedModule)
)]
public class MessageDomainModule : AbpModule
{
}
