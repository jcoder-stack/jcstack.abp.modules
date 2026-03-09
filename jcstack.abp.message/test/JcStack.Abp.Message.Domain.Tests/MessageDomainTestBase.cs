using Volo.Abp.Modularity;

namespace JcStack.Abp.Message;

/// <summary>
/// Domain 层测试基类
/// </summary>
public abstract class MessageDomainTestBase<TStartupModule> : MessageTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
}
