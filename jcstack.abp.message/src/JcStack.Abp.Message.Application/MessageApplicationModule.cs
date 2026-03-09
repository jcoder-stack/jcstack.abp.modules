using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Message;

[DependsOn(
    typeof(MessageDomainModule),
    typeof(MessageApplicationContractsModule),
    typeof(AbpDddApplicationModule)
)]
public class MessageApplicationModule : AbpModule
{
}
