using System.ComponentModel.DataAnnotations;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 创建通知输入
/// </summary>
[Serializable]
public class CreateNotificationInput
{
    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [MaxLength(MessageConsts.MaxTitleLength)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    [Required]
    [MaxLength(MessageConsts.MaxBodyLength)]
    public string Body { get; set; } = null!;

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// 严重程度
    /// </summary>
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Normal;

    /// <summary>
    /// 来源模块
    /// </summary>
    [MaxLength(MessageConsts.MaxSourceModuleLength)]
    public string? SourceModule { get; set; }

    /// <summary>
    /// 来源事件
    /// </summary>
    [MaxLength(MessageConsts.MaxSourceEventLength)]
    public string? SourceEvent { get; set; }

    /// <summary>
    /// 关联实体类型
    /// </summary>
    [MaxLength(MessageConsts.MaxEntityTypeLength)]
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
    /// 发送渠道
    /// </summary>
    public ChannelType Channels { get; set; } = ChannelType.All;
}
