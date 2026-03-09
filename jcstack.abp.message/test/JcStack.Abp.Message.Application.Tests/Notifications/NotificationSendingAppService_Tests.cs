using JcStack.Abp.Message.Notifications;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// NotificationSendingAppService 测试（抽象）
/// </summary>
public abstract class NotificationSendingAppService_Tests<TStartupModule> : MessageApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly INotificationSendingAppService NotificationSendingAppService;
    protected readonly INotificationRepository NotificationRepository;
    protected readonly IUserNotificationRepository UserNotificationRepository;

    protected NotificationSendingAppService_Tests()
    {
        NotificationSendingAppService = GetRequiredService<INotificationSendingAppService>();
        NotificationRepository = GetRequiredService<INotificationRepository>();
        UserNotificationRepository = GetRequiredService<IUserNotificationRepository>();
    }

    [Fact]
    public async Task Should_Send_Notification_To_User()
    {
        // Arrange
        var input = new CreateNotificationInput
        {
            Title = "测试通知发送",
            Body = "这是一条测试通知",
            Type = NotificationType.Info,
            Severity = NotificationSeverity.Normal,
            SourceModule = "TestModule",
            SourceEvent = "TestEvent",
            Channels = ChannelType.SignalR
        };

        // Act
        await NotificationSendingAppService.SendToUserAsync(MessageTestConsts.TestUserId2, input);

        // Assert
        var userNotifications = await UserNotificationRepository.GetListByUserAsync(
            MessageTestConsts.TestUserId2,
            0,
            10);

        userNotifications.ShouldNotBeEmpty();
        userNotifications.ShouldContain(x => x.Notification!.Title == "测试通知发送");
    }

    [Fact]
    public async Task Should_Send_Notification_To_Multiple_Users()
    {
        // Arrange
        var input = new CreateNotificationInput
        {
            Title = "多用户通知测试",
            Body = "这是一条多用户通知",
            Type = NotificationType.Warning,
            Severity = NotificationSeverity.Important,
            SourceModule = "TestModule",
            SourceEvent = "MultiUserTest",
            Channels = ChannelType.SignalR
        };

        var userIds = new[] { MessageTestConsts.TestUserId1, MessageTestConsts.TestUserId2 };

        // Act
        await NotificationSendingAppService.SendToUsersAsync(userIds, input);

        // Assert
        var notifications = await NotificationRepository.GetListAsync();
        notifications.ShouldContain(x => x.Title == "多用户通知测试");
    }
}
