using JcStack.Abp.Message.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace JcStack.Abp.Message.HttpApi;

/// <summary>
/// 消息模块控制器基类
/// </summary>
public abstract class MessageController : AbpControllerBase
{
    protected MessageController()
    {
        LocalizationResource = typeof(MessageResource);
    }
}
