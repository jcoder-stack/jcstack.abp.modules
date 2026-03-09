using JcStack.Abp.Message.Emailing;
using JcStack.Abp.Message.EntityFrameworkCore;
using JcStack.Abp.Message.HttpApi;
using JcStack.Abp.Message.SignalR;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Message.Installer;

/// <summary>
/// 消息模块安装器
/// 用于在宿主应用中快速集成所有消息模块功能
/// </summary>
[DependsOn(
    typeof(MessageApplicationModule),
    typeof(MessageEntityFrameworkCoreModule),
    typeof(MessageHttpApiModule),
    typeof(MessageSignalRModule),
    typeof(MessageEmailingModule))]
public class MessageInstallerModule : AbpModule
{
}
