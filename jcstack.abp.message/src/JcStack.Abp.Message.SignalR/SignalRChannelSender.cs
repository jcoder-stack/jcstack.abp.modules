using JcStack.Abp.Message.Notifications;
using JcStack.Abp.Message.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Message.SignalR;

/// <summary>
/// SignalR 渠道发送器
/// </summary>
public class SignalRChannelSender : IChannelSender, ITransientDependency
{
    public ChannelType Channel => ChannelType.SignalR;
    public int Order => 0; // SignalR 优先级最高

    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRChannelSender> _logger;

    public SignalRChannelSender(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRChannelSender> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<ChannelSendResult> SendAsync(ChannelSendContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new RealTimeNotificationDto
            {
                Id = context.NotificationId,
                Title = context.Title,
                Body = context.Body,
                Type = context.Type,
                Severity = context.Severity,
                SourceModule = context.SourceModule,
                SourceEvent = context.SourceEvent,
                EntityType = context.EntityType,
                EntityId = context.EntityId,
                Data = context.Data,
                CreatedAt = DateTime.UtcNow
            };

            var sentCount = 0;

            if (context.TargetUserIds?.Any() == true)
            {
                // 发送给特定用户列表
                foreach (var userId in context.TargetUserIds)
                {
                    await _hubContext.Clients
                        .User(userId.ToString())
                        .SendAsync("ReceiveNotification", notification, cancellationToken);
                    sentCount++;
                }

                _logger.LogDebug(
                    "SignalR notification sent to {Count} users: {Title}",
                    sentCount,
                    context.Title);
            }
            else if (context.TargetId.HasValue)
            {
                // 发送给单个用户
                await _hubContext.Clients
                    .User(context.TargetId.Value.ToString())
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                sentCount = 1;
                _logger.LogDebug(
                    "SignalR notification sent to user {UserId}: {Title}",
                    context.TargetId,
                    context.Title);
            }
            else
            {
                // 广播给所有用户
                await _hubContext.Clients.All
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                sentCount = -1; // 广播，数量未知
                _logger.LogDebug("SignalR notification broadcast: {Title}", context.Title);
            }

            return ChannelSendResult.Ok(sentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification: {Title}", context.Title);
            return ChannelSendResult.Fail(ex.Message);
        }
    }
}
