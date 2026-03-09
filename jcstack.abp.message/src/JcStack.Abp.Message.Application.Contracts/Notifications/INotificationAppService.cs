using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知应用服务接口（用户端）
/// </summary>
public interface INotificationAppService : IApplicationService
{
    /// <summary>
    /// 获取当前用户通知列表
    /// </summary>
    Task<PagedResultDto<UserNotificationDto>> GetMyNotificationsAsync(GetMyNotificationsInput input);

    /// <summary>
    /// 获取未读数量
    /// </summary>
    Task<int> GetUnreadCountAsync();

    /// <summary>
    /// 标记为已读
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId);

    /// <summary>
    /// 标记全部已读
    /// </summary>
    Task MarkAllAsReadAsync();

    /// <summary>
    /// 批量标记为已读
    /// </summary>
    Task MarkBatchAsReadAsync(List<Guid> notificationIds);

    /// <summary>
    /// 删除通知
    /// </summary>
    Task DeleteAsync(Guid notificationId);
}
