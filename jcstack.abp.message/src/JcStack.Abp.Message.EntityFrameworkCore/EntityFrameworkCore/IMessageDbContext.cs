using JcStack.Abp.Message.Notifications;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.Message.EntityFrameworkCore;

/// <summary>
/// 消息模块 DbContext 接口
/// 支持共享 DbContext 场景
/// </summary>
[ConnectionStringName(MessageDbProperties.ConnectionStringName)]
public interface IMessageDbContext : IEfCoreDbContext
{
    DbSet<Notification> Notifications { get; }
    DbSet<UserNotification> UserNotifications { get; }
    DbSet<UserNotificationSetting> UserNotificationSettings { get; }
}
