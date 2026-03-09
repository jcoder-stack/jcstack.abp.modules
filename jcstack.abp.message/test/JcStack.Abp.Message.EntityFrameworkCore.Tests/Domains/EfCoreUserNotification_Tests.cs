using JcStack.Abp.Message.Notifications;

namespace JcStack.Abp.Message.Domains;

/// <summary>
/// EfCore UserNotification 实体测试（具体实现）
/// </summary>
public class EfCoreUserNotification_Tests : UserNotification_Tests<MessageEntityFrameworkCoreTestModule>
{
    /* Domain tests are written in the base class.
     * Any EF Core-specific tests can be added here if needed.
     */
}
