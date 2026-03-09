using Volo.Abp.Application.Services;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知发送服务实现
/// </summary>
public class NotificationSendingAppService : ApplicationService, INotificationSendingAppService
{
    private readonly INotificationDispatcher _notificationDispatcher;

    public NotificationSendingAppService(INotificationDispatcher notificationDispatcher)
    {
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task SendToUserAsync(Guid userId, CreateNotificationInput input)
    {
        await SendToUsersAsync([userId], input);
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, CreateNotificationInput input)
    {
        var userIdList = userIds.ToList();
        if (userIdList.Count == 0)
        {
            return;
        }

        await _notificationDispatcher.SendAsync(new SendNotificationArgs
        {
            Title = input.Title,
            Body = input.Body,
            Type = input.Type,
            Severity = input.Severity,
            SourceModule = input.SourceModule,
            SourceEvent = input.SourceEvent,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            Data = input.Data,
            TargetType = userIdList.Count == 1 ? NotificationTargetType.User : NotificationTargetType.Users,
            TargetId = userIdList.FirstOrDefault(),
            TargetUserIds = userIdList,
            Channels = input.Channels
        });
    }

    public async Task SendToTenantAsync(Guid? tenantId, CreateNotificationInput input)
    {
        await _notificationDispatcher.SendAsync(new SendNotificationArgs
        {
            Title = input.Title,
            Body = input.Body,
            Type = input.Type,
            Severity = input.Severity,
            SourceModule = input.SourceModule,
            SourceEvent = input.SourceEvent,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            Data = input.Data,
            TargetType = NotificationTargetType.Tenant,
            TargetId = tenantId,
            Channels = input.Channels
        });
    }

    public async Task SendToOrganizationAsync(Guid orgUnitId, CreateNotificationInput input)
    {
        await _notificationDispatcher.SendAsync(new SendNotificationArgs
        {
            Title = input.Title,
            Body = input.Body,
            Type = input.Type,
            Severity = input.Severity,
            SourceModule = input.SourceModule,
            SourceEvent = input.SourceEvent,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            Data = input.Data,
            TargetType = NotificationTargetType.Organization,
            TargetId = orgUnitId,
            Channels = input.Channels
        });
    }
}
