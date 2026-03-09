using Volo.Abp.Reflection;

namespace JcStack.Abp.Message.Permissions;

/// <summary>
/// 消息模块权限定义
/// </summary>
public static class MessagePermissions
{
    public const string GroupName = "Message";

    public static class Notifications
    {
        public const string Default = GroupName + ".Notifications";
        public const string Send = Default + ".Send";
        public const string ManageAll = Default + ".ManageAll";
    }

    public static class Settings
    {
        public const string Default = GroupName + ".Settings";
        public const string Manage = Default + ".Manage";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(MessagePermissions));
    }
}
