using JcStack.Abp.Message.Notifications;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace JcStack.Abp.Message;

/// <summary>
/// 测试数据种子
/// </summary>
public class MessageDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IGuidGenerator _guidGenerator;

    public MessageDataSeedContributor(
        INotificationRepository notificationRepository,
        IUserNotificationRepository userNotificationRepository,
        IGuidGenerator guidGenerator)
    {
        _notificationRepository = notificationRepository;
        _userNotificationRepository = userNotificationRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 创建测试通知1
        var notification1 = new Notification(
            MessageTestConsts.TestNotificationId1,
            MessageTestConsts.TestNotificationTitle1,
            MessageTestConsts.TestNotificationBody1,
            NotificationType.Info,
            NotificationSeverity.Normal,
            NotificationTargetType.User,
            MessageTestConsts.TestUserId1);

        notification1.SetSource("TestModule", "TestEvent");
        await _notificationRepository.InsertAsync(notification1);

        // 创建测试通知2（警告类型）
        var notification2 = new Notification(
            MessageTestConsts.TestNotificationId2,
            "测试警告通知",
            "这是一个警告通知",
            NotificationType.Warning,
            NotificationSeverity.Important,
            NotificationTargetType.All,
            null);

        await _notificationRepository.InsertAsync(notification2);

        // 创建用户通知1（未读）
        var userNotification1 = new UserNotification(
            MessageTestConsts.TestUserNotificationId1,
            MessageTestConsts.TestUserId1,
            MessageTestConsts.TestNotificationId1);

        await _userNotificationRepository.InsertAsync(userNotification1);

        // 创建用户通知2（已读）
        var userNotification2 = new UserNotification(
            MessageTestConsts.TestUserNotificationId2,
            MessageTestConsts.TestUserId1,
            MessageTestConsts.TestNotificationId2);

        userNotification2.MarkAsRead();
        await _userNotificationRepository.InsertAsync(userNotification2);
    }
}
