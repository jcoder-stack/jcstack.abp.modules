namespace JcStack.Abp.Message;

/// <summary>
/// 测试常量
/// </summary>
public static class MessageTestConsts
{
    // 测试用户
    public static readonly Guid TestUserId1 = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    public static readonly Guid TestUserId2 = Guid.Parse("3e701e62-0953-4dd3-910b-dc6cc93ccb0e");
    public static readonly string TestUserName1 = "test_user_1";
    public static readonly string TestUserName2 = "test_user_2";

    // 测试通知
    public static readonly Guid TestNotificationId1 = Guid.Parse("4e701e62-0953-4dd3-910b-dc6cc93ccb0f");
    public static readonly Guid TestNotificationId2 = Guid.Parse("5e701e62-0953-4dd3-910b-dc6cc93ccb10");
    public static readonly string TestNotificationTitle1 = "测试通知标题1";
    public static readonly string TestNotificationBody1 = "测试通知内容1";

    // 测试用户通知
    public static readonly Guid TestUserNotificationId1 = Guid.Parse("6e701e62-0953-4dd3-910b-dc6cc93ccb11");
    public static readonly Guid TestUserNotificationId2 = Guid.Parse("7e701e62-0953-4dd3-910b-dc6cc93ccb12");

    // 测试租户
    public static readonly Guid? TestTenantId = null;

    // 测试组织
    public static readonly Guid TestOrganizationUnitId = Guid.Parse("8e701e62-0953-4dd3-910b-dc6cc93ccb13");
}
