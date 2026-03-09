using JcStack.Abp.Message.Notifications;
using Shouldly;
using Xunit;

namespace JcStack.Abp.Message.Repositories;

/// <summary>
/// EfCore UserNotificationRepository 测试
/// </summary>
public class EfCoreUserNotificationRepository_Tests : MessageTestBase<MessageEntityFrameworkCoreTestModule>
{
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly INotificationRepository _notificationRepository;

    public EfCoreUserNotificationRepository_Tests()
    {
        _userNotificationRepository = GetRequiredService<IUserNotificationRepository>();
        _notificationRepository = GetRequiredService<INotificationRepository>();
    }

    [Fact]
    public async Task Should_Get_List_By_User()
    {
        // Act
        var userNotifications = await _userNotificationRepository.GetListByUserAsync(
            MessageTestConsts.TestUserId1,
            0,
            10,
            includeDetails: true);

        // Assert
        userNotifications.ShouldNotBeNull();
        userNotifications.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Should_Get_Unread_Count()
    {
        // Act
        var count = await _userNotificationRepository.GetUnreadCountAsync(MessageTestConsts.TestUserId1);

        // Assert
        count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Should_Get_Count_By_User()
    {
        // Act
        var count = await _userNotificationRepository.GetCountByUserAsync(MessageTestConsts.TestUserId1);

        // Assert
        count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Should_Filter_By_State()
    {
        // Act - 只获取未读
        var unreadNotifications = await _userNotificationRepository.GetListByUserAsync(
            MessageTestConsts.TestUserId1,
            0,
            10,
            UserNotificationState.Unread);

        // Assert
        unreadNotifications.ShouldNotBeNull();
        unreadNotifications.ShouldAllBe(x => x.State == UserNotificationState.Unread);
    }

    [Fact]
    public async Task Should_Mark_As_Read()
    {
        // Act
        await _userNotificationRepository.MarkAsReadAsync(
            MessageTestConsts.TestUserId1,
            MessageTestConsts.TestNotificationId1);

        // Assert
        var userNotification = await _userNotificationRepository.FindByUserAndNotificationAsync(
            MessageTestConsts.TestUserId1,
            MessageTestConsts.TestNotificationId1);

        userNotification.ShouldNotBeNull();
        userNotification.State.ShouldBe(UserNotificationState.Read);
    }

    [Fact]
    public async Task Should_Mark_All_As_Read()
    {
        // Arrange - 创建一些未读通知
        var notification = new Notification(
            Guid.NewGuid(),
            "批量已读测试",
            "内容",
            NotificationType.Info,
            NotificationSeverity.Normal,
            NotificationTargetType.User,
            MessageTestConsts.TestUserId2);

        await _notificationRepository.InsertAsync(notification);

        var userNotification = new UserNotification(
            Guid.NewGuid(),
            MessageTestConsts.TestUserId2,
            notification.Id);

        await _userNotificationRepository.InsertAsync(userNotification);

        // Act
        await _userNotificationRepository.MarkAllAsReadAsync(MessageTestConsts.TestUserId2);

        // Assert
        var unreadCount = await _userNotificationRepository.GetUnreadCountAsync(MessageTestConsts.TestUserId2);
        unreadCount.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Find_By_User_And_Notification()
    {
        // Act
        var userNotification = await _userNotificationRepository.FindByUserAndNotificationAsync(
            MessageTestConsts.TestUserId1,
            MessageTestConsts.TestNotificationId1);

        // Assert
        userNotification.ShouldNotBeNull();
        userNotification.UserId.ShouldBe(MessageTestConsts.TestUserId1);
        userNotification.NotificationId.ShouldBe(MessageTestConsts.TestNotificationId1);
    }
}
