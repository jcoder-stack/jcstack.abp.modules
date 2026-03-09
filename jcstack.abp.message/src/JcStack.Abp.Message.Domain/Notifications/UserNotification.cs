using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 用户通知状态实体
/// </summary>
public class UserNotification : CreationAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户 ID
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public virtual Guid UserId { get; protected set; }

    /// <summary>
    /// 通知 ID
    /// </summary>
    public virtual Guid NotificationId { get; protected set; }

    /// <summary>
    /// 通知状态
    /// </summary>
    public virtual UserNotificationState State { get; protected set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public virtual DateTime? ReadTime { get; protected set; }

    /// <summary>
    /// 通知（导航属性）
    /// </summary>
    public virtual Notification? Notification { get; protected set; }

    protected UserNotification()
    {
    }

    public UserNotification(
        Guid id,
        Guid userId,
        Guid notificationId,
        Guid? tenantId = null)
        : base(id)
    {
        UserId = userId;
        NotificationId = notificationId;
        State = UserNotificationState.Unread;
        TenantId = tenantId;
    }

    public UserNotification MarkAsRead()
    {
        if (State == UserNotificationState.Unread)
        {
            State = UserNotificationState.Read;
            ReadTime = DateTime.UtcNow;
        }
        return this;
    }

    public UserNotification MarkAsDeleted()
    {
        State = UserNotificationState.Deleted;
        return this;
    }
}
