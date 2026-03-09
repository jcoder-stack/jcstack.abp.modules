namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 渠道发送器接口
/// </summary>
public interface IChannelSender
{
    /// <summary>
    /// 渠道类型
    /// </summary>
    ChannelType Channel { get; }

    /// <summary>
    /// 执行顺序
    /// </summary>
    int Order { get; }

    /// <summary>
    /// 发送通知
    /// </summary>
    Task<ChannelSendResult> SendAsync(ChannelSendContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// 渠道发送上下文
/// </summary>
public class ChannelSendContext
{
    public Guid NotificationId { get; init; }
    public string Title { get; init; } = null!;
    public string Body { get; init; } = null!;
    public NotificationType Type { get; init; }
    public NotificationSeverity Severity { get; init; }
    public string? SourceModule { get; init; }
    public string? SourceEvent { get; init; }
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public Dictionary<string, object>? Data { get; init; }
    public NotificationTargetType TargetType { get; init; }
    public Guid? TargetId { get; init; }
    public IEnumerable<Guid>? TargetUserIds { get; init; }
    public string? Email { get; init; }

    /// <summary>
    /// 邮件模板名称（可选，默认使用通知模板）
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    /// 自定义模板渲染模型（可选，未指定时自动从上下文字段构建）
    /// </summary>
    public object? TemplateModel { get; init; }
}

/// <summary>
/// 渠道发送结果
/// </summary>
public class ChannelSendResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int SentCount { get; init; }

    public static ChannelSendResult Ok(int sentCount = 1) => new() { Success = true, SentCount = sentCount };
    public static ChannelSendResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
