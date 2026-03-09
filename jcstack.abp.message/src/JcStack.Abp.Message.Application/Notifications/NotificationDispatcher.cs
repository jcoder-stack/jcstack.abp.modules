using System.Text.Json;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知派发器实现
/// </summary>
public class NotificationDispatcher : INotificationDispatcher, ITransientDependency
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IEnumerable<IChannelSender> _channelSenders;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        INotificationRepository notificationRepository,
        IUserNotificationRepository userNotificationRepository,
        IEnumerable<IChannelSender> channelSenders,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        ILogger<NotificationDispatcher> logger)
    {
        _notificationRepository = notificationRepository;
        _userNotificationRepository = userNotificationRepository;
        _channelSenders = channelSenders;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task<Guid> SendAsync(SendNotificationArgs args, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Dispatching notification: {Title}, TargetType: {TargetType}", args.Title, args.TargetType);

        var notificationId = _guidGenerator.Create();
        string? dataJson = args.Data != null ? JsonSerializer.Serialize(args.Data) : null;
        string? targetUserIdsJson = args.TargetUserIds != null ? JsonSerializer.Serialize(args.TargetUserIds) : null;

        // 持久化通知
        if (args.Persist)
        {
            var notification = new Notification(
                notificationId,
                args.Title,
                args.Body,
                args.Type,
                args.Severity,
                args.TargetType,
                args.TargetId,
                targetUserIdsJson,
                args.SourceModule,
                args.SourceEvent,
                args.EntityType,
                args.EntityId,
                dataJson,
                _currentTenant.Id);

            await _notificationRepository.InsertAsync(notification, cancellationToken: cancellationToken);

            // 为目标用户创建通知状态记录
            await CreateUserNotificationsAsync(notification, args, cancellationToken);
        }

        // 发送到各渠道
        var context = new ChannelSendContext
        {
            NotificationId = notificationId,
            Title = args.Title,
            Body = args.Body,
            Type = args.Type,
            Severity = args.Severity,
            SourceModule = args.SourceModule,
            SourceEvent = args.SourceEvent,
            EntityType = args.EntityType,
            EntityId = args.EntityId,
            Data = args.Data,
            TargetType = args.TargetType,
            TargetId = args.TargetId,
            TargetUserIds = args.TargetUserIds,
            Email = args.TargetEmail,
            TemplateName = args.TemplateName,
            TemplateModel = args.TemplateModel
        };

        var tasks = _channelSenders
            .Where(s => args.Channels.HasFlag(s.Channel))
            .OrderBy(s => s.Order)
            .Select(s => SendToChannelAsync(s, context, cancellationToken));

        await Task.WhenAll(tasks);

        return notificationId;
    }

    private async Task CreateUserNotificationsAsync(
        Notification notification,
        SendNotificationArgs args,
        CancellationToken cancellationToken)
    {
        var userIds = args.TargetType switch
        {
            NotificationTargetType.User when args.TargetId.HasValue => new[] { args.TargetId.Value },
            NotificationTargetType.Users when args.TargetUserIds != null => args.TargetUserIds.ToArray(),
            // Tenant 和 Organization 需要查询用户，这里暂时跳过
            _ => Array.Empty<Guid>()
        };

        foreach (var userId in userIds)
        {
            var userNotification = new UserNotification(
                _guidGenerator.Create(),
                userId,
                notification.Id,
                _currentTenant.Id);

            await _userNotificationRepository.InsertAsync(userNotification, cancellationToken: cancellationToken);
        }
    }

    private async Task SendToChannelAsync(
        IChannelSender sender,
        ChannelSendContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.SendAsync(context, cancellationToken);
            if (!result.Success)
            {
                _logger.LogWarning("Channel {Channel} send failed: {Error}", sender.Channel, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Channel {Channel} send error", sender.Channel);
        }
    }
}
