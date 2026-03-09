using Volo.Abp.EventBus;

namespace JcStack.Abp.Message.Etos;

/// <summary>
/// 通知发送完成事件
/// </summary>
[EventName("JcStack.Message.NotificationSent")]
[Serializable]
public class NotificationSentEto
{
    /// <summary>
    /// 通知 ID
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 目标类型
    /// </summary>
    public NotificationTargetType TargetType { get; set; }

    /// <summary>
    /// 目标 ID
    /// </summary>
    public Guid? TargetId { get; set; }

    /// <summary>
    /// 接收者数量
    /// </summary>
    public int RecipientCount { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    public Guid? TenantId { get; set; }
}
