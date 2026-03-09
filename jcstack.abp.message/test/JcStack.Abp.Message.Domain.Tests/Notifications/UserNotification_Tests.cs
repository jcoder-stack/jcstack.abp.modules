using JcStack.Abp.Message.Notifications;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// UserNotification 实体测试（抽象）
/// </summary>
public abstract class UserNotification_Tests<TStartupModule> : MessageDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    [Fact]
    public void Should_Create_UserNotification_Successfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        // Act
        var userNotification = new UserNotification(id, userId, notificationId);

        // Assert
        userNotification.Id.ShouldBe(id);
        userNotification.UserId.ShouldBe(userId);
        userNotification.NotificationId.ShouldBe(notificationId);
        userNotification.State.ShouldBe(UserNotificationState.Unread);
        userNotification.ReadTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Mark_As_Read_Successfully()
    {
        // Arrange
        var userNotification = new UserNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        userNotification.MarkAsRead();

        // Assert
        userNotification.State.ShouldBe(UserNotificationState.Read);
        userNotification.ReadTime.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Mark_As_Deleted_Successfully()
    {
        // Arrange
        var userNotification = new UserNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        userNotification.MarkAsDeleted();

        // Assert
        userNotification.State.ShouldBe(UserNotificationState.Deleted);
    }

    [Fact]
    public void Should_Not_Change_ReadTime_When_Already_Read()
    {
        // Arrange
        var userNotification = new UserNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        userNotification.MarkAsRead();
        var firstReadTime = userNotification.ReadTime;

        // 等待一小段时间后再次标记
        Thread.Sleep(10);
        userNotification.MarkAsRead();

        // Assert - 第二次标记不应改变 ReadTime
        userNotification.ReadTime.ShouldBe(firstReadTime);
    }
}
