using JcStack.Abp.Message.Notifications;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace JcStack.Abp.Message.EntityFrameworkCore;

/// <summary>
/// 消息模块实体映射配置扩展
/// </summary>
public static class MessageDbContextModelCreatingExtensions
{
    public static void ConfigureMessage(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Notification>(b =>
        {
            b.ToTable(MessageDbProperties.DbTablePrefix + "Notifications", MessageDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(MessageConsts.MaxTitleLength);

            b.Property(x => x.Body)
                .IsRequired()
                .HasMaxLength(MessageConsts.MaxBodyLength);

            b.Property(x => x.SourceModule)
                .HasMaxLength(MessageConsts.MaxSourceModuleLength);

            b.Property(x => x.SourceEvent)
                .HasMaxLength(MessageConsts.MaxSourceEventLength);

            b.Property(x => x.EntityType)
                .HasMaxLength(MessageConsts.MaxEntityTypeLength);

            b.HasIndex(x => new { x.TenantId, x.CreationTime });
            b.HasIndex(x => x.SourceModule);
        });

        builder.Entity<UserNotification>(b =>
        {
            b.ToTable(MessageDbProperties.DbTablePrefix + "UserNotifications", MessageDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => new { x.UserId, x.State });
            b.HasIndex(x => new { x.UserId, x.NotificationId }).IsUnique();

            b.HasOne(x => x.Notification)
                .WithMany()
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<UserNotificationSetting>(b =>
        {
            b.ToTable(MessageDbProperties.DbTablePrefix + "UserNotificationSettings", MessageDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.SourceModule)
                .HasMaxLength(MessageConsts.MaxSourceModuleLength);

            b.HasIndex(x => x.UserId);
        });
    }
}
