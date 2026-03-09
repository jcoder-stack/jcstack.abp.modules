namespace JcStack.Abp.Message;

/// <summary>
/// 消息模块常量
/// </summary>
public static class MessageConsts
{
    /// <summary>
    /// 标题最大长度
    /// </summary>
    public const int MaxTitleLength = 200;

    /// <summary>
    /// 内容最大长度
    /// </summary>
    public const int MaxBodyLength = 2000;

    /// <summary>
    /// 来源模块最大长度
    /// </summary>
    public const int MaxSourceModuleLength = 100;

    /// <summary>
    /// 来源事件最大长度
    /// </summary>
    public const int MaxSourceEventLength = 100;

    /// <summary>
    /// 实体类型最大长度
    /// </summary>
    public const int MaxEntityTypeLength = 200;

    /// <summary>
    /// Hub 路由常量
    /// </summary>
    public static class HubRoutes
    {
        /// <summary>
        /// 通知 Hub 路由
        /// </summary>
        public const string Notifications = "/signalr-hubs/notifications";
    }

    /// <summary>
    /// SignalR 事件方法名称
    /// </summary>
    public static class HubMethods
    {
        /// <summary>
        /// 接收通知
        /// </summary>
        public const string ReceiveNotification = "ReceiveNotification";

        /// <summary>
        /// 未读数量变化
        /// </summary>
        public const string UnreadCountChanged = "UnreadCountChanged";
    }

    /// <summary>
    /// SignalR 分组前缀
    /// </summary>
    public static class GroupPrefixes
    {
        public const string User = "user:";
        public const string Tenant = "tenant:";
        public const string Organization = "org:";
    }
}
