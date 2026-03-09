using JcStack.Abp.Message.Notifications;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// Notification 实体测试（抽象）
/// </summary>
public abstract class Notification_Tests<TStartupModule> : MessageDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    [Fact]
    public void Should_Create_Notification_Successfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "测试通知";
        var body = "通知内容";

        // Act
        var notification = new Notification(
            id,
            title,
            body,
            NotificationType.Info,
            NotificationSeverity.Normal,
            NotificationTargetType.User,
            MessageTestConsts.TestUserId1);

        // Assert
        notification.Id.ShouldBe(id);
        notification.Title.ShouldBe(title);
        notification.Body.ShouldBe(body);
        notification.Type.ShouldBe(NotificationType.Info);
        notification.Severity.ShouldBe(NotificationSeverity.Normal);
        notification.TargetType.ShouldBe(NotificationTargetType.User);
        notification.TargetId.ShouldBe(MessageTestConsts.TestUserId1);
    }

    [Fact]
    public void Should_Set_Source_Successfully()
    {
        // Arrange
        var notification = new Notification(
            Guid.NewGuid(),
            "标题",
            "内容",
            NotificationType.Info,
            NotificationSeverity.Normal,
            NotificationTargetType.All,
            null);

        // Act
        notification.SetSource("AfterSales", "OrderCreated");

        // Assert
        notification.SourceModule.ShouldBe("AfterSales");
        notification.SourceEvent.ShouldBe("OrderCreated");
    }

    [Fact]
    public void Should_Set_Entity_Reference_Successfully()
    {
        // Arrange
        var notification = new Notification(
            Guid.NewGuid(),
            "标题",
            "内容",
            NotificationType.Info,
            NotificationSeverity.Normal,
            NotificationTargetType.All,
            null);
        var entityId = Guid.NewGuid();

        // Act
        notification.SetEntityReference("AfterSales.Order", entityId);

        // Assert
        notification.EntityType.ShouldBe("AfterSales.Order");
        notification.EntityId.ShouldBe(entityId);
    }

    [Fact]
    public void Should_Throw_When_Title_Is_Empty()
    {
        // Assert
        Should.Throw<ArgumentException>(() =>
        {
            new Notification(
                Guid.NewGuid(),
                "",  // 空标题
                "内容",
                NotificationType.Info,
                NotificationSeverity.Normal,
                NotificationTargetType.All,
                null);
        });
    }

    [Fact]
    public void Should_Throw_When_Body_Is_Empty()
    {
        // Assert
        Should.Throw<ArgumentException>(() =>
        {
            new Notification(
                Guid.NewGuid(),
                "标题",
                "",  // 空内容
                NotificationType.Info,
                NotificationSeverity.Normal,
                NotificationTargetType.All,
                null);
        });
    }
}
