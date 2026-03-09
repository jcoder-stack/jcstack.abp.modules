namespace JcStack.Abp.Message;

/// <summary>
/// 用户通知状态
/// </summary>
public enum UserNotificationState
{
    /// <summary>
    /// 未读
    /// </summary>
    Unread = 0,

    /// <summary>
    /// 已读
    /// </summary>
    Read = 1,

    /// <summary>
    /// 已删除
    /// </summary>
    Deleted = 2
}
