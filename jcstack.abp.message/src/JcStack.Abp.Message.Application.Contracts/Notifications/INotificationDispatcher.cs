namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知派发器接口
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// 发送通知（持久化 + 实时推送）
    /// </summary>
    Task<Guid> SendAsync(SendNotificationArgs args, CancellationToken cancellationToken = default);
}

/// <summary>
/// 发送通知参数
/// </summary>
public class SendNotificationArgs
{
    /// <summary>
    /// 标题
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 内容
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; init; } = NotificationType.Info;

    /// <summary>
    /// 严重程度
    /// </summary>
    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Normal;

    /// <summary>
    /// 来源模块
    /// </summary>
    public string? SourceModule { get; init; }

    /// <summary>
    /// 来源事件
    /// </summary>
    public string? SourceEvent { get; init; }

    /// <summary>
    /// 关联实体类型
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// 关联实体 ID
    /// </summary>
    public Guid? EntityId { get; init; }

    /// <summary>
    /// 扩展数据
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// 目标类型
    /// </summary>
    public NotificationTargetType TargetType { get; init; }

    /// <summary>
    /// 目标 ID（用户/租户/组织 ID）
    /// </summary>
    public Guid? TargetId { get; init; }

    /// <summary>
    /// 目标用户 ID 列表（多用户场景）
    /// </summary>
    public IEnumerable<Guid>? TargetUserIds { get; init; }

    /// <summary>
    /// 目标邮箱地址
    /// </summary>
    public string? TargetEmail { get; init; }

    /// <summary>
    /// 发送渠道
    /// </summary>
    public ChannelType Channels { get; init; } = ChannelType.SignalR;

    /// <summary>
    /// 是否持久化（默认 true）
    /// </summary>
    public bool Persist { get; init; } = true;

    /// <summary>
    /// 邮件模板名称（可选）
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    /// 自定义模板渲染模型（可选）
    /// </summary>
    public object? TemplateModel { get; init; }
}
