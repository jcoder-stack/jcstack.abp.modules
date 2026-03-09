namespace JcStack.Abp.Message;

/// <summary>
/// 消息模块数据库属性
/// </summary>
public static class MessageDbProperties
{
    /// <summary>
    /// 连接字符串名称
    /// 单体模式：可与主应用共享连接字符串
    /// 微服务模式：配置独立数据库连接
    /// </summary>
    public const string ConnectionStringName = "Message";

    /// <summary>
    /// 表前缀
    /// </summary>
    public const string DbTablePrefix = "Msg";

    /// <summary>
    /// Schema（PostgreSQL）
    /// </summary>
    public const string? DbSchema = null;
}
