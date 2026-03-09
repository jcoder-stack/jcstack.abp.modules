namespace JcStack.Abp.AuditLogging.Permissions;

public static class AuditLoggingPermissions
{
    public const string GroupName = "JcStackAbpAuditLogging";

    public static class AuditLogs
    {
        public const string Default = GroupName + ".AuditLogs";
        public const string EntityChanges = Default + ".EntityChanges";
    }
}
