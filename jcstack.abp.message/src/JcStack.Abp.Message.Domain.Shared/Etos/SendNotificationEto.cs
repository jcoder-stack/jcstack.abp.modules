using Volo.Abp.EventBus;

namespace JcStack.Abp.Message.Etos;

/// <summary>
/// 发送通知事件（用于跨服务通信）
/// </summary>
[EventName("JcStack.Message.SendNotification")]
[Serializable]
public class SendNotificationEto
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    public string Body { get; set; } = null!;

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 严重程度
    /// </summary>
    public NotificationSeverity Severity { get; set; }

    /// <summary>
    /// 来源模块
    /// </summary>
    public string? SourceModule { get; set; }

    /// <summary>
    /// 来源事件
    /// </summary>
    public string? SourceEvent { get; set; }

    /// <summary>
    /// 关联实体类型
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// 关联实体 ID
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// 扩展数据
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// 目标类型
    /// </summary>
    public NotificationTargetType TargetType { get; set; }

    /// <summary>
    /// 目标 ID（用户/租户/组织 ID）
    /// </summary>
    public Guid? TargetId { get; set; }

    /// <summary>
    /// 目标用户 ID 列表（多用户场景）
    /// </summary>
    public List<Guid>? TargetUserIds { get; set; }

    /// <summary>
    /// 目标邮箱地址
    /// </summary>
    public string? TargetEmail { get; set; }

    /// <summary>
    /// 发送渠道
    /// </summary>
    public ChannelType Channels { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 是否持久化
    /// </summary>
    public bool Persist { get; set; } = true;
}
