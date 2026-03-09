using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 获取我的通知列表输入
/// </summary>
[Serializable]
public class GetMyNotificationsInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 通知状态过滤
    /// </summary>
    public UserNotificationState? State { get; set; }

    /// <summary>
    /// 来源模块过滤
    /// </summary>
    public string? SourceModule { get; set; }

    /// <summary>
    /// 通知类型过滤
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// 严重程度过滤
    /// </summary>
    public NotificationSeverity? Severity { get; set; }
}
