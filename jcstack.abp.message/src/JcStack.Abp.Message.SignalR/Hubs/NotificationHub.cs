using JcStack.Abp.Message;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;

namespace JcStack.Abp.Message.SignalR.Hubs;

/// <summary>
/// 通知 SignalR Hub
/// </summary>
[HubRoute(MessageConsts.HubRoutes.Notifications)]
[Authorize]
public class NotificationHub : AbpHub
{
    /// <summary>
    /// 客户端确认收到通知
    /// </summary>
    public async Task AcknowledgeNotification(Guid notificationId)
    {
        // 可扩展：记录通知送达确认
        await Clients.Caller.SendAsync("NotificationAcknowledged", notificationId, CancellationToken.None);
    }
}
