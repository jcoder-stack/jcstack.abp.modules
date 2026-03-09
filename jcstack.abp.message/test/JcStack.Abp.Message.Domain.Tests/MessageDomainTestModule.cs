using Volo.Abp.Modularity;

namespace JcStack.Abp.Message;

[DependsOn(
    typeof(MessageTestBaseModule))]
public class MessageDomainTestModule : AbpModule
{
}
