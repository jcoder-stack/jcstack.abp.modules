using JcStack.Abp.Message.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.Message.HttpApi.Notifications;

/// <summary>
/// 通知 API 控制器
/// </summary>
[Area("message")]
[RemoteService(Name = "Message")]
[Route("api/message/notifications")]
[Authorize]
public class NotificationController : MessageController
{
    private readonly INotificationAppService _notificationAppService;

    public NotificationController(INotificationAppService notificationAppService)
    {
        _notificationAppService = notificationAppService;
    }

    /// <summary>
    /// 获取我的通知列表
    /// </summary>
    [HttpGet("my")]
    public Task<PagedResultDto<UserNotificationDto>> GetMyNotificationsAsync([FromQuery] GetMyNotificationsInput input)
    {
        return _notificationAppService.GetMyNotificationsAsync(input);
    }

    /// <summary>
    /// 获取未读数量
    /// </summary>
    [HttpGet("unread-count")]
    public Task<int> GetUnreadCountAsync()
    {
        return _notificationAppService.GetUnreadCountAsync();
    }

    /// <summary>
    /// 标记为已读
    /// </summary>
    [HttpPost("{notificationId}/mark-read")]
    public Task MarkAsReadAsync(Guid notificationId)
    {
        return _notificationAppService.MarkAsReadAsync(notificationId);
    }

    /// <summary>
    /// 标记全部已读
    /// </summary>
    [HttpPost("mark-all-read")]
    public Task MarkAllAsReadAsync()
    {
        return _notificationAppService.MarkAllAsReadAsync();
    }

    /// <summary>
    /// 批量标记为已读
    /// </summary>
    [HttpPost("mark-batch-read")]
    public Task MarkBatchAsReadAsync([FromBody] List<Guid> notificationIds)
    {
        return _notificationAppService.MarkBatchAsReadAsync(notificationIds);
    }

    /// <summary>
    /// 删除通知
    /// </summary>
    [HttpDelete("{notificationId}")]
    public Task DeleteAsync(Guid notificationId)
    {
        return _notificationAppService.DeleteAsync(notificationId);
    }
}
