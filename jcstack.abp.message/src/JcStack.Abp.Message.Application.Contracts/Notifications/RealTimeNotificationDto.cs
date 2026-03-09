namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 实时通知 DTO（推送给前端）
/// </summary>
[Serializable]
public class RealTimeNotificationDto
{
    /// <summary>
    /// 通知 ID（用于标记已读）
    /// </summary>
    public Guid Id { get; set; }

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
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
