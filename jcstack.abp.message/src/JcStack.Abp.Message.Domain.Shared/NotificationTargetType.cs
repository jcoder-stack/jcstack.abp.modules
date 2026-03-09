namespace JcStack.Abp.Message;

/// <summary>
/// 通知目标类型
/// </summary>
public enum NotificationTargetType
{
    /// <summary>
    /// 单个用户
    /// </summary>
    User = 0,

    /// <summary>
    /// 多个用户
    /// </summary>
    Users = 1,

    /// <summary>
    /// 租户广播
    /// </summary>
    Tenant = 2,

    /// <summary>
    /// 组织广播
    /// </summary>
    Organization = 3,

    /// <summary>
    /// 全局广播
    /// </summary>
    All = 4
}
