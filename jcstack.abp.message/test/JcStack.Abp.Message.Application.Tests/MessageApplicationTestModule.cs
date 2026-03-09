using Volo.Abp.Modularity;

namespace JcStack.Abp.Message;

[DependsOn(
    typeof(MessageApplicationModule),
    typeof(MessageDomainTestModule))]
public class MessageApplicationTestModule : AbpModule
{
}
