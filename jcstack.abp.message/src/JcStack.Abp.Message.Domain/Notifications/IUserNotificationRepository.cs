using Volo.Abp.Domain.Repositories;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 用户通知仓储接口
/// </summary>
public interface IUserNotificationRepository : IBasicRepository<UserNotification, Guid>
{
    /// <summary>
    /// 获取用户通知列表
    /// </summary>
    Task<List<UserNotification>> GetListByUserAsync(
        Guid userId,
        int skipCount,
        int maxResultCount,
        UserNotificationState? state = null,
        NotificationType? type = null,
        NotificationSeverity? severity = null,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户通知数量
    /// </summary>
    Task<long> GetCountByUserAsync(
        Guid userId,
        UserNotificationState? state = null,
        NotificationType? type = null,
        NotificationSeverity? severity = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取未读数量
    /// </summary>
    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户通知
    /// </summary>
    Task<UserNotification?> FindByUserAndNotificationAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记为已读
    /// </summary>
    Task MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记全部已读
    /// </summary>
    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量标记为已读
    /// </summary>
    Task MarkBatchAsReadAsync(
        Guid userId,
        List<Guid> notificationIds,
        CancellationToken cancellationToken = default);
}
