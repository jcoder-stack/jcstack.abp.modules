using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知实体
/// </summary>
public class Notification : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户 ID
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 标题
    /// </summary>
    public virtual string Title { get; protected set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    public virtual string Body { get; protected set; } = null!;

    /// <summary>
    /// 通知类型
    /// </summary>
    public virtual NotificationType Type { get; protected set; }

    /// <summary>
    /// 严重程度
    /// </summary>
    public virtual NotificationSeverity Severity { get; protected set; }

    /// <summary>
    /// 来源模块
    /// </summary>
    public virtual string? SourceModule { get; protected set; }

    /// <summary>
    /// 来源事件
    /// </summary>
    public virtual string? SourceEvent { get; protected set; }

    /// <summary>
    /// 关联实体类型
    /// </summary>
    public virtual string? EntityType { get; protected set; }

    /// <summary>
    /// 关联实体 ID
    /// </summary>
    public virtual Guid? EntityId { get; protected set; }

    /// <summary>
    /// 扩展数据 JSON
    /// </summary>
    public virtual string? Data { get; protected set; }

    /// <summary>
    /// 目标类型
    /// </summary>
    public virtual NotificationTargetType TargetType { get; protected set; }

    /// <summary>
    /// 目标 ID（用户/租户/组织 ID）
    /// </summary>
    public virtual Guid? TargetId { get; protected set; }

    /// <summary>
    /// 目标用户 ID 列表 JSON
    /// </summary>
    public virtual string? TargetUserIds { get; protected set; }

    protected Notification()
    {
    }

    public Notification(
        Guid id,
        string title,
        string body,
        NotificationType type = NotificationType.Info,
        NotificationSeverity severity = NotificationSeverity.Normal,
        NotificationTargetType targetType = NotificationTargetType.User,
        Guid? targetId = null,
        string? targetUserIds = null,
        string? sourceModule = null,
        string? sourceEvent = null,
        string? entityType = null,
        Guid? entityId = null,
        string? data = null,
        Guid? tenantId = null)
        : base(id)
    {
        SetTitle(title);
        SetBody(body);
        Type = type;
        Severity = severity;
        TargetType = targetType;
        TargetId = targetId;
        TargetUserIds = targetUserIds;
        SourceModule = sourceModule?.Truncate(MessageConsts.MaxSourceModuleLength);
        SourceEvent = sourceEvent?.Truncate(MessageConsts.MaxSourceEventLength);
        EntityType = entityType?.Truncate(MessageConsts.MaxEntityTypeLength);
        EntityId = entityId;
        Data = data;
        TenantId = tenantId;
    }

    public Notification SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), MessageConsts.MaxTitleLength);
        return this;
    }

    public Notification SetBody(string body)
    {
        Body = Check.NotNullOrWhiteSpace(body, nameof(body), MessageConsts.MaxBodyLength);
        return this;
    }

    public Notification SetSource(string? sourceModule, string? sourceEvent)
    {
        SourceModule = sourceModule?.Truncate(MessageConsts.MaxSourceModuleLength);
        SourceEvent = sourceEvent?.Truncate(MessageConsts.MaxSourceEventLength);
        return this;
    }

    public Notification SetEntityReference(string? entityType, Guid? entityId)
    {
        EntityType = entityType?.Truncate(MessageConsts.MaxEntityTypeLength);
        EntityId = entityId;
        return this;
    }
}
