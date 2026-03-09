using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 用户通知设置实体
/// </summary>
public class UserNotificationSetting : Entity<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户 ID
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public virtual Guid UserId { get; protected set; }

    /// <summary>
    /// 来源模块过滤（null 表示全部）
    /// </summary>
    public virtual string? SourceModule { get; protected set; }

    /// <summary>
    /// 通知类型过滤（null 表示全部）
    /// </summary>
    public virtual NotificationType? NotificationType { get; protected set; }

    /// <summary>
    /// 接收渠道
    /// </summary>
    public virtual ChannelType Channel { get; protected set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; protected set; }

    protected UserNotificationSetting()
    {
    }

    public UserNotificationSetting(
        Guid id,
        Guid userId,
        ChannelType channel = ChannelType.All,
        bool isEnabled = true,
        string? sourceModule = null,
        NotificationType? notificationType = null,
        Guid? tenantId = null)
        : base(id)
    {
        UserId = userId;
        Channel = channel;
        IsEnabled = isEnabled;
        SourceModule = sourceModule;
        NotificationType = notificationType;
        TenantId = tenantId;
    }

    public UserNotificationSetting SetChannel(ChannelType channel)
    {
        Channel = channel;
        return this;
    }

    public UserNotificationSetting SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }
}
