using JcStack.Abp.Message.Notifications;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.Message.EntityFrameworkCore;

/// <summary>
/// 消息模块 DbContext
/// </summary>
[ConnectionStringName(MessageDbProperties.ConnectionStringName)]
public class MessageDbContext : AbpDbContext<MessageDbContext>, IMessageDbContext
{
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<UserNotification> UserNotifications { get; set; } = null!;
    public DbSet<UserNotificationSetting> UserNotificationSettings { get; set; } = null!;

    public MessageDbContext(DbContextOptions<MessageDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureMessage();
    }
}
