using System.Text.Json;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知应用服务实现
/// </summary>
public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IUserNotificationRepository _userNotificationRepository;

    public NotificationAppService(IUserNotificationRepository userNotificationRepository)
    {
        _userNotificationRepository = userNotificationRepository;
    }

    public async Task<PagedResultDto<UserNotificationDto>> GetMyNotificationsAsync(GetMyNotificationsInput input)
    {
        var userId = CurrentUser.GetId();

        var totalCount = await _userNotificationRepository.GetCountByUserAsync(
            userId, input.State, input.Type, input.Severity);

        var items = await _userNotificationRepository.GetListByUserAsync(
            userId,
            input.SkipCount,
            input.MaxResultCount,
            input.State,
            input.Type,
            input.Severity,
            includeDetails: true);

        var dtos = items.Select(x => new UserNotificationDto
        {
            Id = x.Id,
            NotificationId = x.NotificationId,
            Title = x.Notification?.Title ?? string.Empty,
            Body = x.Notification?.Body ?? string.Empty,
            Type = x.Notification?.Type ?? NotificationType.Info,
            Severity = x.Notification?.Severity ?? NotificationSeverity.Normal,
            SourceModule = x.Notification?.SourceModule,
            SourceEvent = x.Notification?.SourceEvent,
            EntityType = x.Notification?.EntityType,
            EntityId = x.Notification?.EntityId,
            Data = DeserializeData(x.Notification?.Data),
            State = x.State,
            CreationTime = x.CreationTime,
            ReadTime = x.ReadTime
        }).ToList();

        return new PagedResultDto<UserNotificationDto>(totalCount, dtos);
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var userId = CurrentUser.GetId();
        return await _userNotificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var userId = CurrentUser.GetId();
        await _userNotificationRepository.MarkAsReadAsync(userId, notificationId);
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = CurrentUser.GetId();
        await _userNotificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task MarkBatchAsReadAsync(List<Guid> notificationIds)
    {
        var userId = CurrentUser.GetId();
        await _userNotificationRepository.MarkBatchAsReadAsync(userId, notificationIds);
    }

    public async Task DeleteAsync(Guid notificationId)
    {
        var userId = CurrentUser.GetId();
        var userNotification = await _userNotificationRepository.FindByUserAndNotificationAsync(userId, notificationId);

        if (userNotification != null)
        {
            userNotification.MarkAsDeleted();
            await _userNotificationRepository.UpdateAsync(userNotification);
        }
    }

    private static Dictionary<string, object>? DeserializeData(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(data);
        }
        catch
        {
            return null;
        }
    }
}
