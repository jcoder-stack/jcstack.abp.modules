using JcStack.Abp.Message.Notifications;

namespace JcStack.Abp.Message.Applications;

/// <summary>
/// EfCore NotificationAppService 测试（具体实现）
/// </summary>
public class EfCoreNotificationAppService_Tests : NotificationAppService_Tests<MessageEntityFrameworkCoreTestModule>
{
    /* Application tests are written in the base class.
     * Any EF Core-specific tests can be added here if needed.
     */
}
