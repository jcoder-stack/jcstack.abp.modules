using JcStack.Abp.Message.Notifications;
using Shouldly;
using Xunit;

namespace JcStack.Abp.Message.Repositories;

/// <summary>
/// EfCore NotificationRepository 测试
/// </summary>
public class EfCoreNotificationRepository_Tests : MessageTestBase<MessageEntityFrameworkCoreTestModule>
{
    private readonly INotificationRepository _notificationRepository;

    public EfCoreNotificationRepository_Tests()
    {
        _notificationRepository = GetRequiredService<INotificationRepository>();
    }

    [Fact]
    public async Task Should_Get_Notification_By_Id()
    {
        // Act
        var notification = await _notificationRepository.GetAsync(MessageTestConsts.TestNotificationId1);

        // Assert
        notification.ShouldNotBeNull();
        notification.Title.ShouldBe(MessageTestConsts.TestNotificationTitle1);
    }

    [Fact]
    public async Task Should_Get_Notification_List()
    {
        // Act
        var notifications = await _notificationRepository.GetListAsync();

        // Assert
        notifications.ShouldNotBeNull();
        notifications.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Should_Insert_Notification()
    {
        // Arrange
        var notification = new Notification(
            Guid.NewGuid(),
            "新通知",
            "新通知内容",
            NotificationType.Success,
            NotificationSeverity.Normal,
            NotificationTargetType.User,
            MessageTestConsts.TestUserId1);

        // Act
        await _notificationRepository.InsertAsync(notification);

        // Assert
        var result = await _notificationRepository.GetAsync(notification.Id);
        result.ShouldNotBeNull();
        result.Title.ShouldBe("新通知");
    }

    [Fact]
    public async Task Should_Delete_Notification()
    {
        // Arrange
        var notification = new Notification(
            Guid.NewGuid(),
            "待删除通知",
            "待删除内容",
            NotificationType.Info,
            NotificationSeverity.Normal,
            NotificationTargetType.All,
            null);

        await _notificationRepository.InsertAsync(notification);

        // Act
        await _notificationRepository.DeleteAsync(notification.Id);

        // Assert
        var result = await _notificationRepository.FindAsync(notification.Id);
        result.ShouldBeNull();
    }
}
