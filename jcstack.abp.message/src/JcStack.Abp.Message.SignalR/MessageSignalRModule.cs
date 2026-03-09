using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Message.SignalR;

[DependsOn(
    typeof(MessageApplicationModule),
    typeof(AbpAspNetCoreSignalRModule))]
public class MessageSignalRModule : AbpModule
{
}
