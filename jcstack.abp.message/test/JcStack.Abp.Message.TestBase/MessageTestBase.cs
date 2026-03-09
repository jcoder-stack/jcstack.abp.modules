using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace JcStack.Abp.Message;

/// <summary>
/// 消息模块测试基类
/// </summary>
public abstract class MessageTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }
}
