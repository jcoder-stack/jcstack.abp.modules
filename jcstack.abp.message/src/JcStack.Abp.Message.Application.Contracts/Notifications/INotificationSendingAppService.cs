using Volo.Abp.Application.Services;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知发送服务接口
/// </summary>
public interface INotificationSendingAppService : IApplicationService
{
    /// <summary>
    /// 发送通知给指定用户
    /// </summary>
    Task SendToUserAsync(Guid userId, CreateNotificationInput input);

    /// <summary>
    /// 发送通知给多个用户
    /// </summary>
    Task SendToUsersAsync(IEnumerable<Guid> userIds, CreateNotificationInput input);

    /// <summary>
    /// 发送通知给租户所有用户
    /// </summary>
    Task SendToTenantAsync(Guid? tenantId, CreateNotificationInput input);

    /// <summary>
    /// 发送通知给组织所有用户
    /// </summary>
    Task SendToOrganizationAsync(Guid orgUnitId, CreateNotificationInput input);
}
