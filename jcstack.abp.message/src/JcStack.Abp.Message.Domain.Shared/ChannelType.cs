namespace JcStack.Abp.Message;

/// <summary>
/// 通知渠道类型
/// </summary>
[Flags]
public enum ChannelType
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,

    /// <summary>
    /// SignalR 实时推送
    /// </summary>
    SignalR = 1,

    /// <summary>
    /// 邮件
    /// </summary>
    Email = 2,

    /// <summary>
    /// 短信（预留）
    /// </summary>
    Sms = 4,

    /// <summary>
    /// 推送通知（预留）
    /// </summary>
    Push = 8,

    /// <summary>
    /// 所有渠道
    /// </summary>
    All = SignalR | Email | Sms | Push
}
