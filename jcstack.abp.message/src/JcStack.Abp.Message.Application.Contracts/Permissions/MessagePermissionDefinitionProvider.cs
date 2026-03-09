using JcStack.Abp.Message.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace JcStack.Abp.Message.Permissions;

/// <summary>
/// 消息模块权限定义提供者
/// </summary>
public class MessagePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var messageGroup = context.AddGroup(
            MessagePermissions.GroupName,
            L("Permission:Message"));

        var notificationsPermission = messageGroup.AddPermission(
            MessagePermissions.Notifications.Default,
            L("Permission:Message.Notifications"));

        notificationsPermission.AddChild(
            MessagePermissions.Notifications.Send,
            L("Permission:Message.Notifications.Send"));

        notificationsPermission.AddChild(
            MessagePermissions.Notifications.ManageAll,
            L("Permission:Message.Notifications.ManageAll"));

        var settingsPermission = messageGroup.AddPermission(
            MessagePermissions.Settings.Default,
            L("Permission:Message.Settings"));

        settingsPermission.AddChild(
            MessagePermissions.Settings.Manage,
            L("Permission:Message.Settings.Manage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MessageResource>(name);
    }
}
