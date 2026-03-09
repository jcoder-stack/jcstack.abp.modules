using JcStack.Abp.Message.Notifications;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// NotificationAppService 测试（抽象）
/// </summary>
public abstract class NotificationAppService_Tests<TStartupModule> : MessageApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly INotificationAppService NotificationAppService;
    protected readonly IUserNotificationRepository UserNotificationRepository;

    protected NotificationAppService_Tests()
    {
        NotificationAppService = GetRequiredService<INotificationAppService>();
        UserNotificationRepository = GetRequiredService<IUserNotificationRepository>();
    }

    [Fact]
    public async Task Should_Get_My_Notifications()
    {
        // Arrange
        var input = new GetMyNotificationsInput
        {
            SkipCount = 0,
            MaxResultCount = 10
        };

        // Act
        var result = await NotificationAppService.GetMyNotificationsAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Get_Unread_Count()
    {
        // Act
        var count = await NotificationAppService.GetUnreadCountAsync();

        // Assert
        count.ShouldBeGreaterThanOrEqualTo(0);
    }
}
